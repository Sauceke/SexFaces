using ExtensibleSaveFormat;
using Illusion.Game;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KK_SexFaces
{
    public class SexFacesController : CharaCustomFunctionController
    {
        public static readonly string[] TRIGGERS =
            { nameof(OnForeplay), nameof(OnInsert), nameof(OnOrgasm) };

        public Dictionary<string, FacialExpression> SexFaces { get; } =
            new Dictionary<string, FacialExpression>();

        internal void OnForeplay(SaveData.Heroine.HExperienceKind experience)
        {
            ApplySexFace(GetSexFaceId(nameof(OnForeplay), experience));
        }

        internal void OnInsert(SaveData.Heroine.HExperienceKind experience)
        {
            ApplySexFace(GetSexFaceId(nameof(OnInsert), experience));
        }

        internal void OnOrgasm(SaveData.Heroine.HExperienceKind experience)
        {
            ApplySexFace(GetSexFaceId(nameof(OnOrgasm), experience));
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

        internal void ApplyEyePreset(int index)
        {
            ChaControl.eyesCtrl.ChangeFace(ExpressionPresets.eyeExpressions.Values.ElementAt(index), true);
        }

        internal void ApplyMouthPreset(int index)
        {
            ChaControl.mouthCtrl.ChangeFace(ExpressionPresets.mouthExpressions.Values.ElementAt(index), true);
        }

        internal void RegisterCurrent(string trigger, SaveData.Heroine.HExperienceKind experience)
        {
            SexFaces[GetSexFaceId(trigger, experience)] =
                FacialExpression.Capture(MakerAPI.GetCharacterControl());
            Utils.Sound.Play(SystemSE.ok_s);
        }

        internal void PreviewSexFace(string trigger, SaveData.Heroine.HExperienceKind experience)
        {
            KK_SexFaces.Hooks.FacialExpressionLock.Locked = false;
            KK_SexFaces.Hooks.EyeDirectionLock.Locked = false;
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
            return from trigger in TRIGGERS
                   from experience in Enum.GetValues(typeof(SaveData.Heroine.HExperienceKind))
                        .Cast<SaveData.Heroine.HExperienceKind>()
                   select GetSexFaceId(trigger, experience);
        }

        private void ApplySexFace(string id)
        {
            KK_SexFaces.Hooks.FacialExpressionLock.Locked = false;
            KK_SexFaces.Hooks.EyeDirectionLock.Locked = false;
            if (!SexFaces.TryGetValue(id, out var face))
            {
                return;
            }
            face.Apply(ChaControl);
            KK_SexFaces.Hooks.FacialExpressionLock.Locked = true;
            KK_SexFaces.Hooks.EyeDirectionLock.Locked = face.EyesTargetPos != null;
        }

        private static string GetSexFaceId(string trigger,
            SaveData.Heroine.HExperienceKind experience)
        {
            return "sexFace(" + trigger + "," + (int)experience + ")";
        }
    }
}
