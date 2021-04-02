using BepInEx.Harmony;
using HarmonyLib;
using KKAPI.Studio;

namespace KK_SexFaces
{
    internal static class Hooks
    {
        public static void InstallHooks()
        {
            if (!StudioAPI.InsideStudio)
            {
                HarmonyWrapper.PatchAll(typeof(HSceneTriggers));
                HarmonyWrapper.PatchAll(typeof(FacialExpressionLock));
                HarmonyWrapper.PatchAll(typeof(EyeDirectionLock));
            }
        }

        public static class FacialExpressionLock
        {
            public static bool Locked { get; set; } = false;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyebrowPtn))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyebrowOpenMax))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesPtn))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesOpenMax))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesBlinkFlag))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookEyesPtn))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeMouthPtn))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeMouthOpenMax))]
            static bool CanExecute()
            {
                return !Locked;
            }
        }

        public static class EyeDirectionLock
        {
            public static bool Locked { get; set; } = false;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookEyesTarget))]
            static bool CanExecute()
            {
                return !Locked;
            }
        }

        private static class HSceneTriggers
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddAibuOrg))]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuOrg))]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.AddSonyuAnalOrg))]
            public static void Orgasm(HFlag __instance)
            {
                GetController(__instance)
                    .OnOrgasm(GetHeroine(__instance).HExperience);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.SetInsertKokan))]
            [HarmonyPatch(typeof(HFlag), nameof(HFlag.SetInsertAnal))]
            public static void Insert(HFlag __instance)
            {
                GetController(__instance)
                    .OnInsert(GetHeroine(__instance).HExperience);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSprite), nameof(HSprite.InitHeroine))]
            [HarmonyPatch(typeof(HSprite), nameof(HSprite.SetAibuStart))]
            public static void Foreplay(HSprite __instance)
            {
                GetController(__instance.flags)
                    .OnForeplay(GetHeroine(__instance.flags).HExperience);
            }

            private static SaveData.Heroine GetHeroine(HFlag hflag)
            {
                return hflag.lstHeroine[0];
            }

            private static SexFacesController GetController(HFlag hflag)
            {
                return GetHeroine(hflag).chaCtrl.GetComponent<SexFacesController>();
            }
        }
    }
}
