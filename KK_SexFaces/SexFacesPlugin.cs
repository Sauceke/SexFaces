using BepInEx;
using BepInEx.Logging;
using AutoVersioning;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using BepInEx.Configuration;
using UnityEngine;

namespace SexFaces
{
    [BepInPlugin(GUID, "Sex Faces", VersionInfo.Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    internal class SexFacesPlugin : BaseUnityPlugin
    {
        public const string GUID = "Sauceke.SexFaces";
        private const int switchTimeLowLimit = 1;
        private const int switchTimeHighLimit = 60;

        public static new ManualLogSource Logger;
        public static ConfigEntry<int> MinSwitchTimeSecs;
        public static ConfigEntry<int> MaxSwitchTimeSecs;

        private void Start()
        {
            Logger = base.Logger;
            Hooks.InstallHooks();
            GameAPI.RegisterExtraBehaviour<GameController>(GUID);
            CharacterApi.RegisterExtraBehaviour<SexFacesController>(GUID);
            SexFacesGui.Instance.Init(this);
            string timing = "Timing";
            Config.Bind(
                section: "",
                key: "Ignore this",
                defaultValue: 0,
                new ConfigDescription(
                    "",
                    tags: new ConfigurationManagerAttributes
                    {
                        CustomDrawer = HelpDrawer,
                        HideSettingName = true,
                        HideDefaultButton = true
                    }));
            MinSwitchTimeSecs = Config.Bind(
                section: timing,
                key: "Minimum switch time (seconds)",
                defaultValue: 5,
                new ConfigDescription(
                    "Minimum timeout for switching between faces.",
                    new AcceptableValueRange<int>(switchTimeLowLimit, switchTimeHighLimit),
                    new ConfigurationManagerAttributes
                    {
                        CustomDrawer = entry => { },
                        HideSettingName = true,
                        HideDefaultButton = true
                    }));
            MaxSwitchTimeSecs = Config.Bind(
                section: timing,
                key: "Maximum switch time (seconds)",
                defaultValue: 20,
                new ConfigDescription(
                    "Maximum timeout for switching between faces.",
                    new AcceptableValueRange<int>(switchTimeLowLimit, switchTimeHighLimit),
                    new ConfigurationManagerAttributes
                    {
                        CustomDrawer = entry => { },
                        HideSettingName = true,
                        HideDefaultButton = true
                    }));
            Config.Bind(
                section: timing,
                key: "Face Switch Time Range",
                defaultValue: "Ignore this",
                new ConfigDescription(
                    "Switch between faces at random intervals between these two values.",
                    tags: new ConfigurationManagerAttributes
                    {
                        CustomDrawer = SwitchTimeRangeDrawer,
                        HideDefaultButton = true
                    }));
        }

        private void HelpDrawer(ConfigEntryBase obj)
        {
            GUILayout.Label("To access the Sex Faces menu, go to Char Maker > ❤ > SexFaces.",
                GUILayout.ExpandWidth(true));
        }

        private void SwitchTimeRangeDrawer(ConfigEntryBase obj)
        {
            float labelWidth = GUI.skin.label.CalcSize(
                new GUIContent($"{switchTimeHighLimit} sec")).x;
            GUILayout.BeginHorizontal();
            {
                float lower = MinSwitchTimeSecs.Value;
                float upper = MaxSwitchTimeSecs.Value;
                GUILayout.Label(lower + " sec", GUILayout.Width(labelWidth));
                RangeSlider.Create(ref lower, ref upper, switchTimeLowLimit, switchTimeHighLimit);
                GUILayout.Label(upper + " sec", GUILayout.Width(labelWidth));
                if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                {
                    lower = (int)MinSwitchTimeSecs.DefaultValue;
                    upper = (int)MaxSwitchTimeSecs.DefaultValue;
                }
                MinSwitchTimeSecs.Value = (int)lower;
                MaxSwitchTimeSecs.Value = (int)upper;
            }
            GUILayout.EndHorizontal();
        }
    }
}
