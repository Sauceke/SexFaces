using KKAPI.Maker;
using KKAPI.Maker.UI;
using System.Collections;
using UniRx;
using UnityEngine;

namespace KK_SexFaces
{
    internal static class SexFacesGui
    {
        public static readonly string[] TRIGGER_DESCRIPTIONS = { "Foreplay", "Penetration", "Orgasm" };
        public static readonly string[] EXP_DESCRIPTIONS =
            { "First Time", "Amateur", "Pro", "Lewd" };

        public static void Init(SexFacesPlugin plugin)
        {
            MakerAPI.RegisterCustomSubCategories +=
                (sender, args) => RegisterMakerControls(plugin, args);
        }

        private static void RegisterMakerControls(SexFacesPlugin plugin, RegisterSubCategoriesEvent e)
        {
            // Doesn't apply to male characters
            if (MakerAPI.GetMakerSex() == 0)
            {
                return;
            }
            var cat = new MakerCategory(MakerConstants.Parameter.Character.CategoryName, "Sex Faces");
            e.AddSubCategory(cat);
            e.AddControl(new MakerText(
                "Set the facial expression using the sidebar.\n" +
                "Then use this menu to add it as a sex face.",
                cat, plugin))
                .TextColor = new Color(0.7f, 0.7f, 0.7f);
            var trigger = e.AddControl(
                new MakerRadioButtons(cat, plugin, "Show during:", TRIGGER_DESCRIPTIONS));
            var experience = e.AddControl(
                new MakerRadioButtons(cat, plugin, "Experience:", EXP_DESCRIPTIONS));
            e.AddControl(new MakerButton("Register", cat, plugin))
                .OnClick.AddListener(() => GetController().RegisterCurrent(
                    SexFacesController.Triggers[trigger.Value],
                    (SaveData.Heroine.HExperienceKind)experience.Value));
            e.AddControl(new MakerButton("View", cat, plugin))
                .OnClick.AddListener(() => GetController().PreviewSexFace(
                    SexFacesController.Triggers[trigger.Value],
                    (SaveData.Heroine.HExperienceKind)experience.Value));
            e.AddControl(new MakerSeparator(cat, plugin));
            e.AddControl(new MakerText("Extra Expression Controls", cat, plugin));
            e.AddControl(new MakerDropdown("Extra Eyebrow Expressions",
                GetDictKeys(ExpressionPresets.eyebrowExpressions), cat, 0, plugin))
                .ValueChanged.Subscribe(GetController().ApplyEyebrowPreset);
            e.AddControl(new MakerDropdown("Extra Eye Expressions",
                GetDictKeys(ExpressionPresets.eyeExpressions), cat, 0, plugin))
                .ValueChanged.Subscribe(GetController().ApplyEyePreset);
            e.AddControl(new MakerDropdown("Extra Mouth Expressions",
                GetDictKeys(ExpressionPresets.mouthExpressions), cat, 0, plugin))
                .ValueChanged.Subscribe(GetController().ApplyMouthPreset);
            e.AddControl(new MakerSlider(cat, "Eyebrow Limit", 0, 1, 1, plugin))
                .ValueChanged.Subscribe(GetController().ChaControl.ChangeEyebrowOpenMax);
            e.AddControl(new MakerText(
                "Sets how much the eyebrows are allowed to move\n" +
                "when the character blinks.",
                cat, plugin))
                .TextColor = new Color(0.7f, 0.7f, 0.7f); ;
            e.AddControl(new MakerSlider(cat, "o_O Scale", 0, 1, .5f, plugin))
                .ValueChanged.Subscribe(GetController().Squint);
            e.AddControl(new MakerText(
                "Makes one eye wider than the other.\n" +
                "For basic eye expressions only (the ones listed\n" +
                "in the Operation Panel).",
                cat, plugin))
                .TextColor = new Color(0.7f, 0.7f, 0.7f);
            e.AddControl(new MakerSlider(cat, "Left Iris Scale", 0, 2, 1, plugin))
                .ValueChanged.Subscribe(GetController().ChangeLeftIrisScale);
            e.AddControl(new MakerSlider(cat, "Right Iris Scale", 0, 2, 1, plugin))
                .ValueChanged.Subscribe(GetController().ChangeRightIrisScale);
        }

        private static SexFacesController GetController()
        {
            return MakerAPI.GetCharacterControl().gameObject.GetComponent<SexFacesController>();
        }

        private static string[] GetDictKeys(IDictionary dict)
        {
            string[] keys = new string[dict.Keys.Count];
            dict.Keys.CopyTo(keys, 0);
            return keys;
        }
    }
}
