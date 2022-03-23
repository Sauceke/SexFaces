using ChaCustom;
using Illusion.Game;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Collections;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SexFaces
{
    internal static class SexFacesGui
    {
        public static readonly string[] TRIGGER_DESCRIPTIONS =
            { "Foreplay", "Penetration", "Orgasm" };
        public static readonly string[] EXP_DESCRIPTIONS =
            { "First Time", "Amateur", "Pro", "Lewd" };
        private static readonly ColorBlock selectedButtonColors = new ColorBlock
        {
            normalColor = new Color(1f, 0.5f, 0f),
            highlightedColor = new Color(1f, 0.5f, 0f),
            pressedColor = new Color(1f, 0.5f, 0f),
            disabledColor = new Color(1f, 0.5f, 0f),
            colorMultiplier = 1f
        };
        private static readonly ColorBlock defaultButtonColors = ColorBlock.defaultColorBlock;

        private static SexFacesController Controller =>
            MakerAPI.GetCharacterControl().gameObject.GetComponent<SexFacesController>();

        private static CustomCheckWindow CheckWindow =>
            UnityEngine.Object.FindObjectOfType<CustomCharaFile>().checkWindow;

        private static MakerRadioButtons triggerButtons;

        private static MakerRadioButtons experienceButtons;

        private static MakerButton deleteButton;

        private static MakerText faceListHeader;

        private static MakerButton[] faceButtons;

        private static int selectedFaceSlot = -1;

        public static void Init(SexFacesPlugin plugin)
        {
            MakerAPI.RegisterCustomSubCategories +=
                (sender, args) => RegisterMakerControls(plugin, args);
            MakerAPI.MakerFinishedLoading +=
                (sender, args) => RefreshFaceList();
        }

        private static void RegisterMakerControls(SexFacesPlugin plugin, RegisterSubCategoriesEvent e)
        {
            var cat = new MakerCategory(MakerConstants.Parameter.Character.CategoryName, "Sex Faces");
            e.AddSubCategory(cat);
            e.AddControl(new MakerText(
                "Set the facial expression using the sidebar.\n" +
                "Then use this menu to add it as a sex face.",
                cat, plugin))
                .TextColor = Color.magenta;
            triggerButtons = e.AddControl(
                new MakerRadioButtons(cat, plugin, "Show during:", TRIGGER_DESCRIPTIONS));
            experienceButtons = e.AddControl(
                new MakerRadioButtons(cat, plugin, "Experience:", EXP_DESCRIPTIONS));
            triggerButtons.ValueChanged.Subscribe(_ => RefreshFaceList());
            experienceButtons.ValueChanged.Subscribe(_ => RefreshFaceList());
            e.AddControl(new MakerButton("Add Current Face", cat, plugin))
                .OnClick.AddListener(AddCurrentFace);
            deleteButton = e.AddControl(new MakerButton("Delete", cat, plugin));
            deleteButton.OnClick.AddListener(ConfirmDeleteFace);
            e.AddControl(new MakerSeparator(cat, plugin));
            faceListHeader = e.AddControl(new MakerText("", cat, plugin));
            faceButtons = Enumerable.Range(0, 10)
                .Select(i => e.AddControl(new MakerButton($"Face #{i + 1}", cat, plugin)))
                .ToArray();
            // workaround for a stupid unity bug
            Action[] onClickActions = faceButtons
                .Select((btn, i) => (Action)(() => OnFaceButtonClicked(i)))
                .ToArray();
            for (int i = 0; i < faceButtons.Length; i++)
            {
                faceButtons[i].OnClick.AddListener(new UnityAction(onClickActions[i]));
            }
            e.AddControl(new MakerSeparator(cat, plugin));
            e.AddControl(new MakerText("Extra Expression Controls", cat, plugin));
            e.AddControl(new MakerDropdown("Extra Eyebrow Expressions",
                GetDictKeys(ExpressionPresets.eyebrowExpressions), cat, 0, plugin))
                .ValueChanged.Subscribe(Controller.ApplyEyebrowPreset);
            e.AddControl(new MakerDropdown("Extra Eye Expressions",
                GetDictKeys(ExpressionPresets.eyeExpressions), cat, 0, plugin))
                .ValueChanged.Subscribe(Controller.ApplyEyePreset);
            e.AddControl(new MakerDropdown("Extra Mouth Expressions",
                GetDictKeys(ExpressionPresets.mouthExpressions), cat, 0, plugin))
                .ValueChanged.Subscribe(Controller.ApplyMouthPreset);
            e.AddControl(new MakerSlider(cat, "Eyebrow Limit", 0, 1, 1, plugin))
                .ValueChanged.Subscribe(Controller.ChaControl.ChangeEyebrowOpenMax);
            e.AddControl(new MakerText(
                "Sets how much the eyebrows are allowed to move\n" +
                "when the character blinks.",
                cat, plugin));
            e.AddControl(new MakerSlider(cat, "o_O Scale", 0, 1, .5f, plugin))
                .ValueChanged.Subscribe(Controller.Squint);
            e.AddControl(new MakerText(
                "Makes one eye wider than the other.\n" +
                "For basic eye expressions only (the ones listed\n" +
                "in the Operation Panel).",
                cat, plugin));
            e.AddControl(new MakerSlider(cat, "Left Iris Scale", 0, 2, 1, plugin))
                .ValueChanged.Subscribe(Controller.ChangeLeftIrisScale);
            e.AddControl(new MakerSlider(cat, "Right Iris Scale", 0, 2, 1, plugin))
                .ValueChanged.Subscribe(Controller.ChangeRightIrisScale);
        }

        private static void AddCurrentFace()
        {
            Controller.AddCurrentFace((SexFacesController.Trigger)triggerButtons.Value,
                    (SaveData.Heroine.HExperienceKind)experienceButtons.Value);
        }

        public static void RefreshFaceList()
        {
            var trigger = (SexFacesController.Trigger)triggerButtons.Value;
            var experience = (SaveData.Heroine.HExperienceKind)experienceButtons.Value;
            var experienceStr = EXP_DESCRIPTIONS[experienceButtons.Value];
            int slots = Controller.GetSlotCount(trigger, experience);
            faceListHeader.Text = slots > 0 ? $"Faces for {trigger}, {experienceStr}:" : "";
            for (int i = 0; i < faceButtons.Length; i++)
            {
                if (faceButtons[i].ControlObject == null)
                {
                    return;
                }
                faceButtons[i].ControlObject.SetActive(i < slots);
            }
            ResetFaceButtonColors();
            deleteButton.ControlObject.GetComponentInChildren<Button>().enabled = slots > 0;
            selectedFaceSlot = -1;
        }

        private static void OnFaceButtonClicked(int index)
        {
            Controller.PreviewSexFace(
                    (SexFacesController.Trigger)triggerButtons.Value,
                    (SaveData.Heroine.HExperienceKind)experienceButtons.Value,
                    slot: index);
            ResetFaceButtonColors();
            faceButtons[index].ControlObject.GetComponentInChildren<Button>().colors =
                selectedButtonColors;
            selectedFaceSlot = index;
        }

        private static void ResetFaceButtonColors()
        {
            foreach (var btn in faceButtons)
            {
                btn.ControlObject.GetComponentInChildren<Button>().colors = defaultButtonColors;
            }
        }

        public static void OfferSaveWithOpenMouth(Action<string> onYes, Action<string> onNo)
        {
            Utils.Sound.Play(SystemSE.window_o);
            CheckWindow.Setup(CustomCheckWindow.CheckType.YesNo,
                    "Mouth must be open for lip sync. Make it open?",
                    strSubMsg: null, strInput: null,
                    onYes, onNo);
        }

        private static void ConfirmDeleteFace()
        {
            Utils.Sound.Play(SystemSE.window_o);
            CheckWindow.Setup(CustomCheckWindow.CheckType.YesNo,
                    $"Delete face #{selectedFaceSlot + 1}?",
                    strSubMsg: null, strInput: null,
                    _ => DeleteFace(), _ => { });
        }

        private static void DeleteFace()
        {
            Controller.DeleteFace((SexFacesController.Trigger)triggerButtons.Value,
                    (SaveData.Heroine.HExperienceKind)experienceButtons.Value,
                    selectedFaceSlot);
            RefreshFaceList();
        }

        private static string[] GetDictKeys(IDictionary dict)
        {
            string[] keys = new string[dict.Keys.Count];
            dict.Keys.CopyTo(keys, 0);
            return keys;
        }
    }
}
