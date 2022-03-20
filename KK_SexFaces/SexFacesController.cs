using ExtensibleSaveFormat;
using Illusion.Game;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SexFaces
{
    internal class SexFacesController : CharaCustomFunctionController
    {
        public static readonly string[] Triggers =
        {
            nameof(OnForeplay), nameof(OnInsert), nameof(OnOrgasm)
        };

        public Dictionary<string, FacialExpression> SexFaces { get; } =
            new Dictionary<string, FacialExpression>();

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

        private IEnumerator PatchFaces()
        {
            // TODO: why the fuck does this not work immediately
            yield return new WaitForSecondsRealtime(1);
            PatchPatternForAhegao(2, newPtnIndex: 43);
            PatchPatternForAhegao(11, newPtnIndex: 44);
            PatchPatternForAhegao(39, newPtnIndex: 45);
            PatchPatternForLopsided(12, leanRight: false, newPtnIndex: 46);
            PatchPatternForLopsided(12, leanRight: true, newPtnIndex: 47);
            PatchPatternForLopsided(27, leanRight: false, newPtnIndex: 48);
            PatchPatternForLopsided(27, leanRight: true, newPtnIndex: 49);
            PatchPatternForLopsided(28, leanRight: false, newPtnIndex: 50);
            PatchPatternForLopsided(28, leanRight: true, newPtnIndex: 51);
        }

        private void OnForeplay(SaveData.Heroine.HExperienceKind experience)
        {
            ApplySexFace(GetSexFaceId(nameof(OnForeplay), experience));
        }

        private void OnInsert(SaveData.Heroine.HExperienceKind experience)
        {
            ApplySexFace(GetSexFaceId(nameof(OnInsert), experience));
        }

        private void OnOrgasm(SaveData.Heroine.HExperienceKind experience)
        {
            ApplySexFace(GetSexFaceId(nameof(OnOrgasm), experience));
        }

        internal void RunLoop(HFlag flags, SaveData.Heroine.HExperienceKind experience)
        {
            StopAllCoroutines();
            StartCoroutine(Loop(flags, experience));
        }

        private IEnumerator Loop(HFlag flags, SaveData.Heroine.HExperienceKind experience)
        {
            Action<SaveData.Heroine.HExperienceKind> currentState = OnForeplay;
            OnForeplay(experience);
            while (!flags.isHSceneEnd)
            {
                Action<SaveData.Heroine.HExperienceKind> newState;
                if (houshiModes.Contains(flags.mode))
                {
                    newState = OnForeplay;
                }
                else if (orgAnimations.Contains(flags.nowAnimStateName))
                {
                    newState = OnOrgasm;
                }
                else if (insertAnimations.Contains(flags.nowAnimStateName))
                {
                    newState = OnInsert;
                }
                else
                {
                    newState = OnForeplay;
                }
                if (currentState != newState)
                {
                    currentState = newState;
                    newState(experience);
                }
                yield return new WaitForSecondsRealtime(0.2f);
            }
            Hooks.FacialExpressionLock.Locked = false;
            Hooks.EyeDirectionLock.Locked = false;
        }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
            if (currentGameMode == GameMode.Studio)
            {
                return;
            }
            var data = new PluginData();
            foreach (var entry in SexFaces)
            {
                data.data[entry.Key] = entry.Value.Serialize();
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
                SexFaces.Clear();
                var data = GetExtendedData();
                if (data != null)
                {
                    foreach (string id in GetAllSexFaceIds())
                    {
                        if (data.data.TryGetValue(id, out var value))
                        {
                            SexFaces[id] = FacialExpression.Deserialize((string)value);
                        }
                    }
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
            var eyeTexW = Mathf.Lerp(1.8f, -0.2f, ChaControl.fileFace.pupilWidth * scale);
            var eyeTexH = Mathf.Lerp(1.8f, -0.2f, ChaControl.fileFace.pupilHeight * scale);
            ChaControl.eyeLookMatCtrl[0].SetEyeTexScaleX(eyeTexW);
            ChaControl.eyeLookMatCtrl[0].SetEyeTexScaleY(eyeTexH);
        }

        internal void ChangeRightIrisScale(float scale)
        {
            var eyeTexW = Mathf.Lerp(1.8f, -0.2f, ChaControl.fileFace.pupilWidth * scale);
            var eyeTexH = Mathf.Lerp(1.8f, -0.2f, ChaControl.fileFace.pupilHeight * scale);
            ChaControl.eyeLookMatCtrl[1].SetEyeTexScaleX(eyeTexW);
            ChaControl.eyeLookMatCtrl[1].SetEyeTexScaleY(eyeTexH);
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

        internal void RegisterCurrent(string trigger, SaveData.Heroine.HExperienceKind experience)
        {
            var face = FacialExpression.Capture(MakerAPI.GetCharacterControl());
            if (face.MouthOpenMax < 0.2f)
            {
                SexFacesGui.ConfirmSaveWithClosedMouth(_ => SaveFace(face, trigger, experience));
                return;
            }
            SaveFace(face, trigger, experience);
        }

        private void SaveFace(FacialExpression face, string trigger,
            SaveData.Heroine.HExperienceKind experience)
        {
            SexFaces[GetSexFaceId(trigger, experience)] = face;
            Utils.Sound.Play(SystemSE.ok_s);
        }

        internal void PreviewSexFace(string trigger, SaveData.Heroine.HExperienceKind experience)
        {
            Hooks.FacialExpressionLock.Locked = false;
            Hooks.EyeDirectionLock.Locked = false;
            if (!SexFaces.TryGetValue(GetSexFaceId(trigger, experience), out var face))
            {
                Utils.Sound.Play(SystemSE.cancel);
                return;
            }
            face.Apply(ChaControl);
            Utils.Sound.Play(SystemSE.sel);
        }

        private static IEnumerable<string> GetAllSexFaceIds()
        {
            return from trigger in Triggers
                   from experience in Enum.GetValues(typeof(SaveData.Heroine.HExperienceKind))
                        .Cast<SaveData.Heroine.HExperienceKind>()
                   select GetSexFaceId(trigger, experience);
        }

        private void ApplySexFace(string id)
        {
            Hooks.FacialExpressionLock.Locked = false;
            Hooks.EyeDirectionLock.Locked = false;
            if (!SexFaces.TryGetValue(id, out var face))
            {
                return;
            }
            face.Apply(ChaControl);
            Hooks.FacialExpressionLock.Locked = true;
            Hooks.EyeDirectionLock.Locked = face.EyesTargetPos != null;
        }

        private static string GetSexFaceId(string trigger,
            SaveData.Heroine.HExperienceKind experience)
        {
            return "sexFace(" + trigger + "," + (int)experience + ")";
        }

        private void PatchPatternForAhegao(int ptnIndex, int newPtnIndex)
        {
            var mouthCtrl = ChaControl.mouthCtrl;
            for (int fbsIndex = 0; fbsIndex < mouthCtrl.FBSTarget.Length; fbsIndex++)
            {
                var fbs = mouthCtrl.FBSTarget[fbsIndex];
                if (newPtnIndex >= fbs.PtnSet.Length)
                {
                    Array.Resize(ref fbs.PtnSet, newPtnIndex + 1);
                    for (int i = 0; i < fbs.PtnSet.Length; i++)
                    {
                        fbs.PtnSet[i] = fbs.PtnSet[i] ?? new FBSTargetInfo.CloseOpen();
                    }
                }
                // tongue out (24) for the tongue controller (4), leave everything else as is
                int ptn = fbsIndex == 4 ? fbs.PtnSet[24].Open : fbs.PtnSet[ptnIndex].Open;
                fbs.PtnSet[newPtnIndex].Open = ptn;
                fbs.PtnSet[newPtnIndex].Close = ptn;
            }
        }

        private void PatchPatternForLopsided(int ptnIndex, bool leanRight, int newPtnIndex)
        {
            var mouthCtrl = ChaControl.mouthCtrl;
            for (int fbsIndex = 0; fbsIndex < mouthCtrl.FBSTarget.Length; fbsIndex++)
            {
                var fbs = mouthCtrl.FBSTarget[fbsIndex];
                if (newPtnIndex >= fbs.PtnSet.Length)
                {
                    Array.Resize(ref fbs.PtnSet, newPtnIndex + 1);
                    for (int i = 0; i < fbs.PtnSet.Length; i++)
                    {
                        fbs.PtnSet[i] = fbs.PtnSet[i] ?? new FBSTargetInfo.CloseOpen();
                    }
                }
                var meshCtrl = fbs.GetSkinnedMeshRenderer();
                var mesh = meshCtrl.sharedMesh;
                int vertCount = mesh.vertexCount;
                var deltaVertsOpen = new Vector3[vertCount];
                var deltaVertsClosed = new Vector3[vertCount];
                var deltaNorms = new Vector3[vertCount];
                var deltaTans = new Vector3[vertCount];
                float halfWidth = mesh.vertices.Max(_ => _.x);
                int openPtn = fbs.PtnSet[ptnIndex].Open;
                int closedPtn = fbs.PtnSet[ptnIndex].Close;
                mesh.GetBlendShapeFrameVertices(openPtn, 0, deltaVertsOpen, deltaNorms, deltaTans);
                mesh.GetBlendShapeFrameVertices(closedPtn, 0, deltaVertsClosed, deltaTans, deltaTans);
                var deltaVertsLopsided = new Vector3[vertCount];
                for (int i = 0; i < vertCount; i++)
                {
                    float relativeX = Mathf.InverseLerp(-halfWidth, halfWidth, mesh.vertices[i].x);
                    float blend = Sigmoid(relativeX);
                    if (leanRight)
                    {
                        blend = 1f - blend;
                    }
                    deltaVertsLopsided[i] = deltaVertsClosed[i] * blend
                        + deltaVertsOpen[i] * (1f - blend);
                }
                string name = "sexfaces.lopsided." + (leanRight ? "right." : "left.")
                    + mesh.GetBlendShapeName(fbs.PtnSet[ptnIndex].Close);
                try
                {
                    mesh.AddBlendShapeFrame(name, 1f, deltaVertsLopsided, deltaNorms, deltaTans);
                }
                catch (ArgumentException)
                {
                    // not noteworthy, just means we have already patched this pattern
                }
                int index = mesh.GetBlendShapeIndex(name);
                fbs.PtnSet[newPtnIndex].Open = index;
                fbs.PtnSet[newPtnIndex].Close = fbs.PtnSet[ptnIndex].Close;
                // this looks stupid but we need to tell unity the mesh was modified
                meshCtrl.sharedMesh = meshCtrl.sharedMesh;
            }
        }

        private float Sigmoid(float x)
        {
            return (float)(Math.Tanh((x - 0.5) * 10) + 1) / 2f;
        }
    }
}
