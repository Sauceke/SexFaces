using KKAPI.Maker;
using KKAPI.Maker.UI;
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
                    SexFacesController.TRIGGERS[trigger.Value],
                    (SaveData.Heroine.HExperienceKind)experience.Value));
            e.AddControl(new MakerButton("View", cat, plugin))
                .OnClick.AddListener(() => GetController().PreviewSexFace(
                    SexFacesController.TRIGGERS[trigger.Value],
                    (SaveData.Heroine.HExperienceKind)experience.Value));
        }

        private static SexFacesController GetController()
        {
            return MakerAPI.GetCharacterControl().gameObject.GetComponent<SexFacesController>();
        }
    }
}
