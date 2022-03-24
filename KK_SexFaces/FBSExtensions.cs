using System;
using System.Linq;
using UnityEngine;

namespace SexFaces
{
    internal static class FBSExtensions
    {
        public static void PatchPatternForAhegao(this FBSCtrlMouth mouthCtrl, int ptnIndex,
            int newPtnIndex)
        {
            for (int fbsIndex = 0; fbsIndex < mouthCtrl.FBSTarget.Length; fbsIndex++)
            {
                var fbs = mouthCtrl.FBSTarget[fbsIndex];
                if (newPtnIndex >= fbs.PtnSet.Length)
                {
                    Array.Resize(ref fbs.PtnSet, newPtnIndex + 1);
                    for (int i = 0; i < fbs.PtnSet.Length; i++)
                    {
                        fbs.PtnSet[i] = fbs.PtnSet[i] ?? new FBSTargetInfo.CloseOpen();
                    }
                }
                // tongue out (24) for the tongue controller (4), leave everything else as is
                int ptn = fbsIndex == 4 ? fbs.PtnSet[24].Open : fbs.PtnSet[ptnIndex].Open;
                fbs.PtnSet[newPtnIndex].Open = ptn;
                fbs.PtnSet[newPtnIndex].Close = ptn;
            }
        }

        public static void PatchPatternForLopsided(this FBSCtrlMouth mouthCtrl, int ptnIndex,
            bool leanRight, int newPtnIndex)
        {
            for (int fbsIndex = 0; fbsIndex < mouthCtrl.FBSTarget.Length; fbsIndex++)
            {
                var fbs = mouthCtrl.FBSTarget[fbsIndex];
                if (newPtnIndex >= fbs.PtnSet.Length)
                {
                    Array.Resize(ref fbs.PtnSet, newPtnIndex + 1);
                    for (int i = 0; i < fbs.PtnSet.Length; i++)
                    {
                        fbs.PtnSet[i] = fbs.PtnSet[i] ?? new FBSTargetInfo.CloseOpen();
                    }
                }
                var meshCtrl = fbs.GetSkinnedMeshRenderer();
                var mesh = meshCtrl.sharedMesh;
                int vertCount = mesh.vertexCount;
                var deltaVertsOpen = new Vector3[vertCount];
                var deltaVertsClosed = new Vector3[vertCount];
                var deltaNorms = new Vector3[vertCount];
                var deltaTans = new Vector3[vertCount];
                float halfWidth = mesh.vertices.Max(_ => _.x);
                int openPtn = fbs.PtnSet[ptnIndex].Open;
                int closedPtn = fbs.PtnSet[ptnIndex].Close;
                mesh.GetBlendShapeFrameVertices(openPtn, 0, deltaVertsOpen, deltaNorms, deltaTans);
                mesh.GetBlendShapeFrameVertices(closedPtn, 0, deltaVertsClosed, deltaNorms,
                    deltaTans);
                var deltaVertsLopsided = new Vector3[vertCount];
                for (int i = 0; i < vertCount; i++)
                {
                    float relativeX = Mathf.InverseLerp(-halfWidth, halfWidth, mesh.vertices[i].x);
                    float blend = Sigmoid(relativeX);
                    if (leanRight)
                    {
                        blend = 1f - blend;
                    }
                    deltaVertsLopsided[i] = deltaVertsClosed[i] * blend
                        + deltaVertsOpen[i] * (1f - blend);
                }
                string name = "sexfaces.lopsided." + (leanRight ? "right." : "left.")
                    + mesh.GetBlendShapeName(fbs.PtnSet[ptnIndex].Close);
                try
                {
                    mesh.AddBlendShapeFrame(name, 1f, deltaVertsLopsided, deltaNorms, deltaTans);
                }
                catch (ArgumentException)
                {
                    // not noteworthy, just means we have already patched this pattern
                }
                int index = mesh.GetBlendShapeIndex(name);
                fbs.PtnSet[newPtnIndex].Open = index;
                fbs.PtnSet[newPtnIndex].Close = fbs.PtnSet[ptnIndex].Close;
                // this looks stupid but we need to tell unity the mesh was modified
                meshCtrl.sharedMesh = meshCtrl.sharedMesh;
            }
        }

        private static float Sigmoid(float x)
        {
            return (float)(Math.Tanh((x - 0.5) * 10) + 1) / 2f;
        }
    }
}
