using HarmonyLib;
using KKAPI.Studio;
using System.Collections.Generic;
using System.Linq;

namespace SexFaces
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
            private static readonly int[] exemptMouthPatterns =
                { (int)MouthPattern.Eating, (int)MouthPattern.HoldInMouth, (int)MouthPattern.Kiss };

            private static readonly HashSet<ChaControl> lockedControls = new HashSet<ChaControl>();

            public static void Lock(ChaControl control)
            {
                lockedControls.Add(control);
            }

            public static void Unlock(ChaControl control)
            {
                lockedControls.Remove(control);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyebrowPtn))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyebrowOpenMax))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesPtn))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesOpenMax))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesBlinkFlag))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookEyesPtn))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeMouthOpenMax))]
            private static bool CanChange(ChaControl __instance)
            {
                return !lockedControls.Contains(__instance);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeMouthPtn))]
            private static bool CanChangeMouth(ChaControl __instance, int ptn)
            {
                return exemptMouthPatterns.Contains(ptn) || !lockedControls.Contains(__instance);
            }
        }

        public static class EyeDirectionLock
        {
            private static readonly HashSet<ChaControl> lockedControls = new HashSet<ChaControl>();

            public static void Lock(ChaControl control)
            {
                lockedControls.Add(control);
            }

            public static void Unlock(ChaControl control)
            {
                lockedControls.Remove(control);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookEyesTarget))]
            private static bool CanChange(ChaControl __instance)
            {
                return !lockedControls.Contains(__instance);
            }
        }
    }
}