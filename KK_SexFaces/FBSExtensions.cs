using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SexFaces
{
    internal static class FBSExtensions
    {
        public static void AddAhegao(this FBSCtrlMouth mouthCtrl, MouthPattern basePtn,
            MouthPattern newPtn)
        {
            var ptns = new Dictionary<FBSIndex, MouthPattern>()
            {
                { FBSIndex.Face, basePtn },
                { FBSIndex.Neck, basePtn },
                { FBSIndex.Teeth, basePtn },
                { FBSIndex.Eyes, basePtn },
                { FBSIndex.Tongue, MouthPattern.TongueOut }
            };
            AddMixed(mouthCtrl, ptns, keepOpen: true, newPtn);
        }
        
        public static void AddClosedTeeth(this FBSCtrlMouth mouthCtrl, MouthPattern basePtn,
            MouthPattern newPtn)
        {
            var ptns = new Dictionary<FBSIndex, MouthPattern>()
            {
                { FBSIndex.Face, basePtn },
                { FBSIndex.Neck, basePtn },
                { FBSIndex.Teeth, MouthPattern.BigI },
                { FBSIndex.Eyes, basePtn },
                { FBSIndex.Tongue, basePtn }
            };
            AddMixed(mouthCtrl, ptns, keepOpen: false, newPtn);
        }
        
        public static void AddLopsided(this FBSCtrlMouth mouthCtrl, MouthPattern basePtn,
            bool leanRight, MouthPattern newPtn)
        {
            for (int fbsIndex = 0; fbsIndex < mouthCtrl.FBSTarget.Length; fbsIndex++)
            {
                var fbs = mouthCtrl.FBSTarget[fbsIndex];
                if ((int)newPtn >= fbs.PtnSet.Length)
                {
                    Array.Resize(ref fbs.PtnSet, (int)newPtn + 1);
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
                int openPtn = fbs.PtnSet[(int)basePtn].Open;
                int closedPtn = fbs.PtnSet[(int)basePtn].Close;
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
                deltaVertsLopsided = fbsIndex == (int)FBSIndex.Face
                    ? deltaVertsLopsided
                    : deltaVertsOpen;
                string name = "sexfaces.lopsided." + (leanRight ? "right." : "left.")
                    + mesh.GetBlendShapeName(fbs.PtnSet[(int)basePtn].Close);
                try
                {
                    mesh.AddBlendShapeFrame(name, 1f, deltaVertsLopsided, deltaNorms, deltaTans);
                }
                catch (ArgumentException)
                {
                    // not noteworthy, just means we have already patched this pattern
                }
                int index = mesh.GetBlendShapeIndex(name);
                fbs.PtnSet[(int)newPtn].Open = index;
                fbs.PtnSet[(int)newPtn].Close = fbs.PtnSet[(int)basePtn].Close;
                // this looks stupid but we need to tell unity the mesh was modified
                meshCtrl.sharedMesh = meshCtrl.sharedMesh;
            }
        }

        private static void AddMixed(FBSCtrlMouth mouthCtrl,
            Dictionary<FBSIndex, MouthPattern> ptns, bool keepOpen, MouthPattern newPtn)
        {
            for (int fbsIndex = 0; fbsIndex < mouthCtrl.FBSTarget.Length; fbsIndex++)
            {
                var fbs = mouthCtrl.FBSTarget[fbsIndex];
                if ((int)newPtn >= fbs.PtnSet.Length)
                {
                    Array.Resize(ref fbs.PtnSet, (int)newPtn + 1);
                    for (int i = 0; i < fbs.PtnSet.Length; i++)
                    {
                        fbs.PtnSet[i] = fbs.PtnSet[i] ?? new FBSTargetInfo.CloseOpen();
                    }
                }
                FBSTargetInfo.CloseOpen closeOpen = fbs.PtnSet[(int)ptns[(FBSIndex)fbsIndex]];
                fbs.PtnSet[(int)newPtn].Open = closeOpen.Open;
                fbs.PtnSet[(int)newPtn].Close = keepOpen ? closeOpen.Open : closeOpen.Close;
            }
        }
        
        private static float Sigmoid(float x) => (float)(Math.Tanh((x - 0.5) * 10) + 1) / 2f;
    }
}