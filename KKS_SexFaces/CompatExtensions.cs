using ChaCustom;
using System;
using System.Collections.Generic;

namespace SexFaces
{
    internal static class CompatExtensions
    {
        public static void ChangeFace(this FBSBase fbs, Dictionary<int, float> face, bool blend)
        {
            fbs.dictFace = face;
            fbs.ChangeFace(blend);
        }

        public static void Setup(this CustomCheckWindow checkWindow,
            CustomCheckWindow.CheckType type, string strMainMsg, string strSubMsg, string strInput,
            params Action<string>[] act)
        {
            checkWindow.Setup(type, "", strMainMsg, strSubMsg, strInput, act);
        }
    }
}
