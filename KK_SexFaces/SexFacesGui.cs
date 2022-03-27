using ChaCustom;
using Illusion.Game;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SexFaces
{
    internal class SexFacesGui
    {
        public static readonly SexFacesGui Instance = new SexFacesGui();
        public static readonly string[] TriggerDescriptions =
            { "Foreplay", "Penetration", "Orgasm" };
        public static readonly string[] ExperienceDescriptions =
            { "First Time", "Amateur", "Pro", "Lewd" };
        private static readonly ColorBlock selectedButtonColors = new ColorBlock
        {
            normalColor = Color.yellow,
            highlightedColor = Color.yellow,
            pressedColor = Color.yellow,
            disabledColor = Color.yellow,
            colorMultiplier = 1f
        };

        private SexFacesController Controller =>
            MakerAPI.GetCharacterControl().gameObject.GetComponent<SexFacesController>();

        private CustomCheckWindow CheckWindow =>
            UnityEngine.Object.FindObjectOfType<CustomCharaFile>().checkWindow;

        private ColorBlock DefaultButtonColors =>
            deleteButton.ControlObject.GetComponentInChildren<Button>().colors;

        private MakerRadioButtons triggerButtons;

        private MakerRadioButtons experienceButtons;

        private MakerButton deleteButton;

        private MakerText faceListHeader;

        private MakerButton[] faceButtons;

        private int selectedFaceSlot = -1;

        private SexFacesGui() { }

        public void Init(SexFacesPlugin plugin)
        {
            MakerAPI.RegisterCustomSubCategories +=
                (sender, args) => RegisterMakerControls(plugin, args);
            MakerAPI.MakerFinishedLoading +=
                (sender, args) => RefreshFaceList();
        }

        private void RegisterMakerControls(SexFacesPlugin plugin, RegisterSubCategoriesEvent e)
        {
            var cat = new MakerCategory(MakerConstants.Parameter.Character.CategoryName,
                "Sex Faces");
            e.AddSubCategory(cat);
            e.AddControl(new MakerText(
                "Set the facial expression using the sidebar.\n" +
                "Then use this menu to add it as a sex face.",
                cat, plugin))
                .TextColor = Color.yellow;
            triggerButtons = e.AddControl(
                new MakerRadioButtons(cat, plugin, "Show during:", TriggerDescriptions));
            experienceButtons = e.AddControl(
                new MakerRadioButtons(cat, plugin, "Experience:", ExperienceDescriptions));
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
            e.AddControl(new MakerText(
                "These are only additional expressions.\n" +
                "You can also use the sidebar ----->",
                cat, plugin))
                .TextColor = Color.yellow;
            e.AddControl(new MakerDropdown("Extra Eyebrow Expressions",
                ExpressionPresets.EyebrowExpressionNames, cat, 0, plugin))
                .ValueChanged.Subscribe(Controller.ApplyEyebrowPreset);
            e.AddControl(new MakerDropdown("Extra Eye Expressions",
                ExpressionPresets.EyeExpressionNames, cat, 0, plugin))
                .ValueChanged.Subscribe(Controller.ApplyEyePreset);
            e.AddControl(new MakerDropdown("Extra Mouth Expressions",
                ExpressionPresets.MouthExpressionNames, cat, 0, plugin))
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

        private void AddCurrentFace()
        {
            var trigger = (SexFacesController.Trigger)triggerButtons.Value;
            var experience = (SaveData.Heroine.HExperienceKind)experienceButtons.Value;
            int slots = Controller.GetSlotCount(trigger, experience);
            if (slots >= faceButtons.Length)
            {
                SexFacesPlugin.Logger.LogMessage(
                    $"Only {faceButtons.Length} faces allowed for each condition.");
                Utils.Sound.Play(SystemSE.cancel);
                return;
            }
            Controller.AddCurrentFace(trigger, experience);
        }

        public void RefreshFaceList()
        {
            var trigger = (SexFacesController.Trigger)triggerButtons.Value;
            var experience = (SaveData.Heroine.HExperienceKind)experienceButtons.Value;
            int slots = Controller.GetSlotCount(trigger, experience);
            var triggerStr = TriggerDescriptions[triggerButtons.Value];
            var experienceStr = ExperienceDescriptions[experienceButtons.Value];
            faceListHeader.Text = slots > 0 ? $"Faces for {experienceStr} {triggerStr}:" : "";
            for (int i = 0; i < faceButtons.Length; i++)
            {
                if (faceButtons[i].ControlObject == null)
                {
                    return;
                }
                faceButtons[i].ControlObject.SetActive(i < slots);
            }
            ResetFaceButtonColors();
            selectedFaceSlot = -1;
        }

        private void OnFaceButtonClicked(int index)
        {
            Controller.PreviewSexFace(
                    (SexFacesController.Trigger)triggerButtons.Value,
                    (SaveData.Heroine.HExperienceKind)experienceButtons.Value,
                    slot: index);
            ResetFaceButtonColors();
            faceButtons[index].ControlObject.GetComponentInChildren<Button>().colors
                = selectedButtonColors;
            faceButtons[index].ControlObject.GetComponentInChildren<Button>().transition
                = Selectable.Transition.ColorTint;
            selectedFaceSlot = index;
        }

        private void ResetFaceButtonColors()
        {
            foreach (var btn in faceButtons)
            {
                btn.ControlObject.GetComponentInChildren<Button>().colors = DefaultButtonColors;
            }
        }

        public void OfferSaveWithOpenMouth(Action<string> onYes, Action<string> onNo)
        {
            Utils.Sound.Play(SystemSE.window_o);
            CheckWindow.Setup(CustomCheckWindow.CheckType.YesNo,
                    "Mouth must be open for lip sync. Make it open?",
                    strSubMsg: null, strInput: null,
                    onYes, onNo);
        }

        private void ConfirmDeleteFace()
        {
            if (selectedFaceSlot < 0)
            {
                SexFacesPlugin.Logger.LogMessage("No face selected to delete.");
                Utils.Sound.Play(SystemSE.cancel);
                return;
            }
            Utils.Sound.Play(SystemSE.window_o);
            CheckWindow.Setup(CustomCheckWindow.CheckType.YesNo,
                    $"Delete face #{selectedFaceSlot + 1}?",
                    strSubMsg: null, strInput: null,
                    _ => DeleteFace(), _ => { });
        }

        private void DeleteFace()
        {
            Controller.DeleteFace((SexFacesController.Trigger)triggerButtons.Value,
                    (SaveData.Heroine.HExperienceKind)experienceButtons.Value,
                    selectedFaceSlot);
            RefreshFaceList();
        }
    }
}
