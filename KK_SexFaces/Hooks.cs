using HarmonyLib;
using KKAPI.Studio;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
                Harmony.CreateAndPatchAll(typeof(NeckLookCalcHooks));
            }
        }

        public static class FacialExpressionLock
        {
            private static readonly int[] exemptMouthPatterns =
                { (int)MouthPattern.Eating, (int)MouthPattern.HoldInMouth, (int)MouthPattern.Kiss };

            private static readonly HashSet<ChaControl> lockedControls = new HashSet<ChaControl>();

            public static void Lock(ChaControl control) => lockedControls.Add(control);
            
            public static void Unlock(ChaControl control) => lockedControls.Remove(control);

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyebrowPtn))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyebrowOpenMax))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesPtn))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesOpenMax))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeEyesBlinkFlag))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookEyesPtn))]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeMouthOpenMax))]
            private static bool CanChange(ChaControl __instance) =>
                !lockedControls.Contains(__instance);

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeMouthPtn))]
            private static bool CanChangeMouth(ChaControl __instance, int ptn) =>
                exemptMouthPatterns.Contains(ptn) || !lockedControls.Contains(__instance);
        }

        public static class EyeDirectionLock
        {
            private static readonly HashSet<ChaControl> lockedControls = new HashSet<ChaControl>();

            public static void Lock(ChaControl control) => lockedControls.Add(control);

            public static void Unlock(ChaControl control) => lockedControls.Remove(control);

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeLookEyesTarget))]
            private static bool CanChange(ChaControl __instance) =>
                !lockedControls.Contains(__instance);
        }

        internal static class NeckLookCalcHooks
        {
            private static readonly Dictionary<NeckLookCalcVer2, Quaternion> angleOffsets =
                new Dictionary<NeckLookCalcVer2, Quaternion>();

            public static Quaternion GetNeckRotation(ChaControl chaControl) =>
                angleOffsets.TryGetValue(chaControl.neckLookCtrl.neckLookScript, out var rotation)
                    ? rotation
                    : Quaternion.identity;
            
            public static void SetNeckRotation(ChaControl chaControl, Quaternion rotation) =>
                angleOffsets[chaControl.neckLookCtrl.neckLookScript] = rotation;
            
            public static void ResetNeckRotation(ChaControl chaControl) =>
                angleOffsets.Remove(chaControl.neckLookCtrl.neckLookScript);
            
            [HarmonyPostfix]
            [HarmonyPatch(typeof(NeckLookCalcVer2), nameof(NeckLookCalcVer2.NeckUpdateCalc))]
            private static void NeckUpdateCalc(NeckLookCalcVer2 __instance)
            {
                if (!angleOffsets.TryGetValue(__instance, out var offset))
                {
                    return;
                }
                foreach (var bone in __instance.aBones)
                {
                    bone.neckBone.rotation *= offset;
                    bone.fixAngle = bone.neckBone.localRotation;
                }
            }
        }
    }
}