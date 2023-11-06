using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SexFaces
{
    public partial class FacialExpression
    {
        public string EyebrowExpression { get; set; }
        public float EyebrowOpenMax { get; set; }
        public string EyeExpression { get; set; }
        public float EyesOpenMax { get; set; }
        public bool EyesBlinkFlag { get; set; }
        public int LookEyesPattern { get; set; }
        public Vector3? EyesTargetPos { get; set; }
        public Quaternion? EyesTargetRot { get; set; }
        public string MouthExpression { get; set; }
        public float MouthOpenMax { get; set; }
        public float LeftEyeScaleX { get; set; } = 1f;
        public float LeftEyeScaleY { get; set; } = 1f;
        public float RightEyeScaleX { get; set; } = 1f;
        public float RightEyeScaleY { get; set; } = 1f;
        public Quaternion? NeckRot { get; set; }

        public static FacialExpression Capture(ChaControl chaControl)
        {
            var eyeTexW = Mathf.Lerp(1.8f, -0.2f, chaControl.fileFace.pupilWidth);
            var eyeTexH = Mathf.Lerp(1.8f, -0.2f, chaControl.fileFace.pupilHeight);
            var leftEyeMatCtrl = chaControl.eyeLookMatCtrl[0];
            var rightEyeMatCtrl = chaControl.eyeLookMatCtrl[1];
            var expression = new FacialExpression
            {
                EyebrowExpression = DictToString(GetExpression(chaControl.eyebrowCtrl)),
                EyebrowOpenMax = chaControl.GetEyebrowOpenMax(),
                EyeExpression = DictToString(GetExpression(chaControl.eyesCtrl)),
                EyesOpenMax = chaControl.GetEyesOpenMax(),
                EyesBlinkFlag = chaControl.GetEyesBlinkFlag(),
                LookEyesPattern = chaControl.GetLookEyesPtn(),
                MouthExpression = DictToString(GetExpression(chaControl.mouthCtrl)),
                MouthOpenMax = chaControl.GetMouthOpenMax(),
                LeftEyeScaleX = leftEyeMatCtrl.GetEyeTexScale().x / eyeTexW,
                LeftEyeScaleY = leftEyeMatCtrl.GetEyeTexScale().y / eyeTexH,
                RightEyeScaleX = rightEyeMatCtrl.GetEyeTexScale().x / eyeTexW,
                RightEyeScaleY = rightEyeMatCtrl.GetEyeTexScale().y / eyeTexH,
                NeckRot = Hooks.NeckLookCalcHooks.GetNeckRotation(chaControl)
            };
            if (IsLookingAtFixedPosition(chaControl))
            {
                expression.EyesTargetPos = chaControl.objEyesLookTarget.transform.localPosition;
                expression.EyesTargetRot = chaControl.objEyesLookTargetP.transform.localRotation;
            }
            return expression;
        }

        public void Apply(ChaControl chaControl)
        {
            chaControl.eyebrowCtrl.ChangeFace(StringToDict(EyebrowExpression), true);
            chaControl.ChangeEyebrowOpenMax(EyebrowOpenMax);
            chaControl.eyesCtrl.ChangeFace(StringToDict(EyeExpression), true);
            chaControl.ChangeEyesOpenMax(EyesOpenMax);
            chaControl.ChangeEyesBlinkFlag(EyesBlinkFlag);
            chaControl.ChangeLookEyesPtn(LookEyesPattern);
            var eyeTexW = Mathf.Lerp(1.8f, -0.2f, chaControl.fileFace.pupilWidth);
            var eyeTexH = Mathf.Lerp(1.8f, -0.2f, chaControl.fileFace.pupilHeight);
            var leftEyeMatCtrl = chaControl.eyeLookMatCtrl[0];
            var rightEyeMatCtrl = chaControl.eyeLookMatCtrl[1];
            leftEyeMatCtrl.SetEyeTexScaleX(eyeTexW * LeftEyeScaleX);
            leftEyeMatCtrl.SetEyeTexScaleY(eyeTexH * LeftEyeScaleY);
            rightEyeMatCtrl.SetEyeTexScaleX(eyeTexW * RightEyeScaleX);
            rightEyeMatCtrl.SetEyeTexScaleY(eyeTexH * RightEyeScaleY);
            if (EyesTargetPos != null)
            {
                chaControl.objEyesLookTargetP.transform.localRotation = EyesTargetRot.Value;
                var target = chaControl.objEyesLookTarget;
                target.transform.localPosition = EyesTargetPos.Value;
                chaControl.ChangeLookEyesTarget(0, target.transform);
            }
            else
            {
                chaControl.eyeLookCtrl.target = Camera.current.transform;
            }
            chaControl.mouthCtrl.ChangeFace(StringToDict(MouthExpression), true);
            chaControl.ChangeMouthOpenMax(MouthOpenMax);
            chaControl.MoveNeck(NeckRot ?? Quaternion.identity);
        }

        private static bool IsLookingAtFixedPosition(ChaControl chaControl)
        {
            var setTarget = chaControl.objEyesLookTarget.transform.localPosition;
            var actualTarget = chaControl.eyeLookCtrl.target.localPosition;
            return (setTarget - actualTarget).magnitude == 0;
        }

        private static Dictionary<int, float> GetExpression(FBSBase fbs) =>
            Traverse.Create(fbs).Field<Dictionary<int, float>>("dictNowFace").Value;

        private static string DictToString(Dictionary<int, float> dict) => dict
            .Select(pair => pair.Key + ":" + pair.Value.ToString("F4"))
            .Aggregate("", (a, b) => a + ";" + b);

        private static Dictionary<int, float> StringToDict(string s) =>
            s.Split(';').Skip(1).ToDictionary(
                pair => int.Parse(pair.Split(':')[0]),
                pair => float.Parse(pair.Split(':')[1]));
        
    }
}