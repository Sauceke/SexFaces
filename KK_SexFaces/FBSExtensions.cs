using System.Collections.Generic;

namespace SexFaces
{
    internal static class FBSExtensions
    {
        public static void SetFace(this FBSBase fbs, Dictionary<int, float> face, bool blend)
        {
            fbs.ChangeFace(face, blend);
        }
    }
}
