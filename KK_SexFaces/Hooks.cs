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
                Harmony.CreateAndPatchAll(typeof(FacialExpressionLock));
                Harmony.CreateAndPatchAll(typeof(EyeDirectionLock));
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
            private static bool CanExecute()
            {
                return !Locked;
            }
        }

        public static class EyeDirectionLock
        {
            public static bool Locked { get; set; } = false;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookEyesTarget))]
            private static bool CanExecute()
            {
                return !Locked;
            }
        }
    }
}
