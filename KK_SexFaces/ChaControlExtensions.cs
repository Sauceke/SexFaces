using System.Collections;
using UnityEngine;

namespace SexFaces
{
    public static class ChaControlExtensions
    {
        public static void MoveNeck(this ChaControl chaControl, Quaternion end)
            => chaControl.StartCoroutine(SlerpNeck(chaControl, end));

        private static IEnumerator SlerpNeck(ChaControl chaControl, Quaternion end)
        {
            const float durationSecs = 1f;
            float startTime = Time.unscaledTime;
            var start = Hooks.NeckLookCalcHooks.GetNeckRotation(chaControl);
            while (Time.unscaledTime - startTime < durationSecs)
            {
                var t = (Time.unscaledTime - startTime) / durationSecs;
                t = 1f - (1f - t) * (1f - t);
                var rotation = Quaternion.Slerp(start, end, t);
                Hooks.NeckLookCalcHooks.SetNeckRotation(chaControl, rotation);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}