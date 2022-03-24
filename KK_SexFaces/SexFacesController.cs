using ExtensibleSaveFormat;
using Illusion.Game;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SexFaces
{
    internal class SexFacesController : CharaCustomFunctionController
    {
        private static readonly string[] insertAnimations =
        {
            "Insert", "A_Insert",
            "WLoop", "SLoop",
            "A_WLoop", "A_SLoop", "A_OLoop",
            "OLoop", "A_OLoop"
        };

        private static readonly string[] orgAnimations =
        {
            "OUT_START", "OUT_LOOP", "IN_START", "IN_LOOP",
            "M_OUT_Start", "M_OUT_Loop", "M_IN_Start", "M_IN_Loop",
            "WS_IN_Start", "WS_IN_Loop", "SS_IN_Start", "SS_IN_Loop",
            "A_WS_IN_Start", "A_WS_IN_Loop", "A_SS_IN_Start", "A_SS_IN_Loop"
        };

        private static readonly HFlag.EMode[] houshiModes =
        {
            HFlag.EMode.houshi, HFlag.EMode.houshi3P, HFlag.EMode.houshi3PMMF
        };

        private HashSet<SexFaceEntry> sexFaces = new HashSet<SexFaceEntry>();

        private System.Random random = new System.Random();

        private IEnumerator PatchFaces()
        {
            // TODO: why the fuck does this not work immediately
            yield return new WaitForSecondsRealtime(1);
            ChaControl.mouthCtrl.PatchPatternForAhegao(2, newPtnIndex: 43);
            ChaControl.mouthCtrl.PatchPatternForAhegao(11, newPtnIndex: 44);
            ChaControl.mouthCtrl.PatchPatternForAhegao(39, newPtnIndex: 45);
            ChaControl.mouthCtrl.PatchPatternForLopsided(12, leanRight: false, newPtnIndex: 46);
            ChaControl.mouthCtrl.PatchPatternForLopsided(12, leanRight: true, newPtnIndex: 47);
            ChaControl.mouthCtrl.PatchPatternForLopsided(27, leanRight: false, newPtnIndex: 48);
            ChaControl.mouthCtrl.PatchPatternForLopsided(27, leanRight: true, newPtnIndex: 49);
            ChaControl.mouthCtrl.PatchPatternForLopsided(28, leanRight: false, newPtnIndex: 50);
            ChaControl.mouthCtrl.PatchPatternForLopsided(28, leanRight: true, newPtnIndex: 51);
        }

        private void OnForeplay(SaveData.Heroine.HExperienceKind experience)
        {
            ApplyRandomSexFace(Trigger.OnForeplay, experience);
        }

        private void OnService(SaveData.Heroine.HExperienceKind experience)
        {
            OnForeplay(experience);
        }

        private void OnKiss(SaveData.Heroine.HExperienceKind experience) { }

        private void OnInsert(SaveData.Heroine.HExperienceKind experience)
        {
            ApplyRandomSexFace(Trigger.OnInsert, experience);
        }

        private void OnOrgasm(SaveData.Heroine.HExperienceKind experience)
        {
            ApplyRandomSexFace(Trigger.OnOrgasm, experience);
        }

        internal void RunLoop(HFlag flags, SaveData.Heroine.HExperienceKind experience,
            HandCtrl hand)
        {
            StopAllCoroutines();
            StartCoroutine(Loop(flags, experience, hand));
        }

        private IEnumerator Loop(HFlag flags, SaveData.Heroine.HExperienceKind experience,
            HandCtrl hand)
        {
            Action<SaveData.Heroine.HExperienceKind> currentState = OnForeplay;
            OnForeplay(experience);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var switchTimeSecs = 0;
            while (!flags.isHSceneEnd)
            {
                var newState = GetSexFaceDisplayAction(flags, hand);
                if (currentState != newState)
                {
                    currentState = newState;
                    newState(experience);
                }
                else if (stopwatch.Elapsed.TotalSeconds > switchTimeSecs)
                {
                    currentState(experience);
                    switchTimeSecs = random.Next(5, 20);
                    stopwatch.Reset();
                    stopwatch.Start();
                }
                yield return new WaitForSecondsRealtime(0.2f);
            }
            Hooks.FacialExpressionLock.Unlock(ChaControl);
            Hooks.EyeDirectionLock.Unlock(ChaControl);
        }

        private Action<SaveData.Heroine.HExperienceKind> GetSexFaceDisplayAction(HFlag flags,
            HandCtrl hand)
        {
            return (hand?.IsKissAction() ?? false) ? OnKiss
                : houshiModes.Contains(flags.mode) ? OnService
                : orgAnimations.Contains(flags.nowAnimStateName) ? OnOrgasm
                : insertAnimations.Contains(flags.nowAnimStateName) ? OnInsert
                : (Action<SaveData.Heroine.HExperienceKind>)OnForeplay;
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (currentGameMode == GameMode.Studio)
            {
                return;
            }
            var data = new PluginData();
            data.version = 1;
            foreach (var entry in sexFaces)
            {
                data.data[GetSexFaceId(entry)] = entry.Face.Serialize();
            }
            SetExtendedData(data);
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            if (maintainState)
            {
                return;
            }
            StartCoroutine(PatchFaces());
            if (!MakerAPI.InsideAndLoaded || MakerAPI.GetCharacterLoadFlags().Parameters)
            {
                sexFaces.Clear();
                var data = GetExtendedData();
                if (data == null)
                {
                    return;
                }
                foreach (var kvp in data.data)
                {
                    ParseSexFaceId(data.version, kvp.Key, out var trigger, out var experience,
                        out int slot);
                    var face = FacialExpression.Deserialize((string)kvp.Value);
                    var faceEntry = new SexFaceEntry
                    {
                        Trigger = trigger,
                        Experience = experience,
                        Slot = slot,
                        Face = face
                    };
                    sexFaces.Add(faceEntry);
                }
            }
        }

        internal void Squint(float squintingFactor)
        {
            // The "correct" formula would be to multiply this by 2, but the upper 50% isn't that
            // interesting anyway (it's basically a wink at that point)
            var winkWeight = Math.Abs(squintingFactor - .5f);
            var newExpression = new Dictionary<int, float>
                {
                    // 5 = left wink
                    // 6 = right wink
                    { squintingFactor < .5f ? 5 : 6,  winkWeight},
                    { ChaControl.GetEyesPtn(), 1 - winkWeight}
                };
            ChaControl.eyesCtrl.ChangeFace(newExpression, true);
        }

        internal void ChangeLeftIrisScale(float scale)
        {
            ChangeIrisScale(0, scale);
        }

        internal void ChangeRightIrisScale(float scale)
        {
            ChangeIrisScale(1, scale);
        }

        private void ChangeIrisScale(int index, float scale)
        {
            // exact formula used by the game
            var eyeTexW = Mathf.Lerp(1.8f, -0.2f, ChaControl.fileFace.pupilWidth * scale);
            var eyeTexH = Mathf.Lerp(1.8f, -0.2f, ChaControl.fileFace.pupilHeight * scale);
            ChaControl.eyeLookMatCtrl[index].SetEyeTexScaleX(eyeTexW);
            ChaControl.eyeLookMatCtrl[index].SetEyeTexScaleY(eyeTexH);
        }

        private void ResetIrisScales()
        {
            ChangeLeftIrisScale(1f);
            ChangeRightIrisScale(1f);
        }

        internal void ApplyEyebrowPreset(int index)
        {
            ChaControl.eyebrowCtrl.ChangeFace(
                ExpressionPresets.eyebrowExpressions.Values.ElementAt(index), false);
        }

        internal void ApplyEyePreset(int index)
        {
            ChaControl.eyesCtrl.ChangeFace(
                ExpressionPresets.eyeExpressions.Values.ElementAt(index), false);
        }

        internal void ApplyMouthPreset(int index)
        {
            ChaControl.mouthCtrl.ChangeFace(
                ExpressionPresets.mouthExpressions.Values.ElementAt(index), false);
        }

        internal void AddCurrentFace(Trigger trigger, SaveData.Heroine.HExperienceKind experience)
        {
            var face = FacialExpression.Capture(MakerAPI.GetCharacterControl());
            var slot = GetSlotCount(trigger, experience);
            var faceEntry = new SexFaceEntry
            {
                Trigger = trigger,
                Experience = experience,
                Slot = slot,
                Face = face
            };
            if (face.MouthOpenMax < 0.5f)
            {
                SexFacesGui.Instance.OfferSaveWithOpenMouth(
                    onYes: _ => SaveFaceWithOpenMouth(faceEntry),
                    onNo: _ => SaveFace(faceEntry));
                return;
            }
            SaveFace(faceEntry);
        }

        private void SaveFace(SexFaceEntry faceEntry)
        {
            sexFaces.Add(faceEntry);
            PreviewSexFace(faceEntry.Trigger, faceEntry.Experience, faceEntry.Slot);
            SexFacesGui.Instance.RefreshFaceList();
        }

        private void SaveFaceWithOpenMouth(SexFaceEntry faceEntry)
        {
            faceEntry.Face.MouthOpenMax = 1f;
            SaveFace(faceEntry);
        }

        internal void DeleteFace(Trigger trigger, SaveData.Heroine.HExperienceKind experience,
            int slot)
        {
            sexFaces = new HashSet<SexFaceEntry>(sexFaces.Where(sf => sf.Trigger != trigger
                || sf.Experience != experience || sf.Slot != slot));
            foreach (var faceEntry in sexFaces)
            {
                if (faceEntry.Trigger == trigger && faceEntry.Experience == experience
                    && faceEntry.Slot > slot)
                {
                    faceEntry.Slot--;
                }
            }
        }

        internal int GetSlotCount(Trigger trigger, SaveData.Heroine.HExperienceKind experience) =>
            sexFaces
                .Where(sf => sf.Trigger == trigger && sf.Experience == experience)
                .Count();

        internal void PreviewSexFace(Trigger trigger, SaveData.Heroine.HExperienceKind experience,
            int slot)
        {
            Hooks.FacialExpressionLock.Unlock(ChaControl);
            Hooks.EyeDirectionLock.Unlock(ChaControl);
            var face = sexFaces
                .Where(sf => sf.Trigger == trigger && sf.Experience == experience
                    && sf.Slot == slot)
                .FirstOrDefault();
            if (face == null)
            {
                Utils.Sound.Play(SystemSE.cancel);
                return;
            }
            face.Face.Apply(ChaControl);
            Utils.Sound.Play(SystemSE.sel);
        }

        private void ApplyRandomSexFace(Trigger trigger,
            SaveData.Heroine.HExperienceKind experience)
        {
            Hooks.FacialExpressionLock.Unlock(ChaControl);
            Hooks.EyeDirectionLock.Unlock(ChaControl);
            var facePool = sexFaces
                .Where(sf => sf.Trigger == trigger && sf.Experience == experience);
            if (facePool.Count() == 0)
            {
                ResetIrisScales();
                return;
            }
            var index = random.Next(facePool.Count());
            var face = facePool.Skip(index).First().Face;
            face.Apply(ChaControl);
            Hooks.FacialExpressionLock.Lock(ChaControl);
            if (face.EyesTargetPos != null)
            {
                Hooks.EyeDirectionLock.Lock(ChaControl);
            }
        }

        private static string GetSexFaceId(SexFaceEntry faceEntry)
            => $"sexFace({faceEntry.Trigger},{(int)faceEntry.Experience},{faceEntry.Slot})";

        private static void ParseSexFaceId(int version, string id, out Trigger trigger,
            out SaveData.Heroine.HExperienceKind experience, out int slot)
        {
            if (version > 1)
            {
                string msg = "Character data uses a later version of SexFaces. Please update the SexFaces plugin.";
                SexFacesPlugin.Logger.LogMessage(msg);
                throw new ArgumentOutOfRangeException(msg);
            }
            string idPattern = version == 0 ? @"^sexFace\(([a-zA-Z]+),([0-3])\)$"
                : @"^sexFace\(([a-zA-Z]+),([0-3]),([0-9]+)\)$";
            var match = Regex.Match(id, idPattern);
            trigger = (Trigger)Enum.Parse(typeof(Trigger), match.Groups[1].Value);
            experience = (SaveData.Heroine.HExperienceKind)int.Parse(match.Groups[2].Value);
            slot = version == 0 ? 0 : int.Parse(match.Groups[3].Value);
        }

        public enum Trigger
        {
            OnForeplay, OnInsert, OnOrgasm
        }

        public class SexFaceEntry
        {
            public Trigger Trigger { get; set; }
            public SaveData.Heroine.HExperienceKind Experience { get; set; }
            public int Slot { get; set; }
            public FacialExpression Face { get; set; }
        }
    }
}
