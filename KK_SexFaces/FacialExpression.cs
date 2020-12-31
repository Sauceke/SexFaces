using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace KK_SexFaces
{
    public class FacialExpression
    {
        public int EyebrowPattern { get; set; }
        public float EyebrowOpenMax { get; set; }
        public int EyesPattern { get; set; }
        public float EyesOpenMax { get; set; }
        public bool EyesBlinkFlag { get; set; }
        public int LookEyesPattern { get; set; }
        public Vector3? EyesTargetPos { get; set; }
        public Quaternion? EyesTargetRot { get; set; }
        public int MouthPattern { get; set; }
        public float MouthOpenMax { get; set; }

        public string Serialize()
        {
            using (StringWriter writer = new StringWriter())
            {
                new XmlSerializer(typeof(FacialExpression)).Serialize(writer, this);
                return writer.ToString();
            }
        }

        public static FacialExpression Deserialize(string blob)
        {
            if (blob == null)
            {
                return null;
            }
            using (StringReader reader = new StringReader(blob))
            {
                return (FacialExpression)new XmlSerializer(typeof(FacialExpression)).Deserialize(reader);
            }
        }

        public static FacialExpression Capture(ChaControl chaControl)
        {
            var expression = new FacialExpression()
            {
                EyebrowPattern = chaControl.GetEyebrowPtn(),
                EyebrowOpenMax = chaControl.GetEyebrowOpenMax(),
                EyesPattern = chaControl.GetEyesPtn(),
                EyesOpenMax = chaControl.GetEyesOpenMax(),
                EyesBlinkFlag = chaControl.GetEyesBlinkFlag(),
                LookEyesPattern = chaControl.GetLookEyesPtn(),
                MouthPattern = chaControl.GetMouthPtn(),
                MouthOpenMax = chaControl.GetMouthOpenMax()
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
            chaControl.ChangeEyebrowPtn(EyebrowPattern, true);
            chaControl.ChangeEyebrowOpenMax(EyebrowOpenMax);
            chaControl.ChangeEyesPtn(EyesPattern, true);
            chaControl.ChangeEyesOpenMax(EyesOpenMax);
            chaControl.ChangeEyesBlinkFlag(EyesBlinkFlag);
            chaControl.ChangeLookEyesPtn(LookEyesPattern);
            if (EyesTargetPos != null)
            {
                chaControl.objEyesLookTargetP.transform.localRotation = EyesTargetRot.Value;
                GameObject eyesTarget = new GameObject();
                eyesTarget.transform.SetParent(chaControl.objEyesLookTargetP.transform);
                eyesTarget.transform.localPosition = EyesTargetPos.Value;
                chaControl.ChangeLookEyesTarget(0, eyesTarget.transform);
            }
            else
            {
                chaControl.eyeLookCtrl.target = Camera.current.transform;
            }
            chaControl.ChangeMouthPtn(MouthPattern, true);
            chaControl.ChangeMouthOpenMax(MouthOpenMax);
        }

        private static bool IsLookingAtFixedPosition(ChaControl chaControl)
        {
            Vector3 setTarget = chaControl.objEyesLookTarget.transform.localPosition;
            Vector3 actualTarget = chaControl.eyeLookCtrl.target.localPosition;
            return (setTarget - actualTarget).magnitude == 0;
        }
    }
}
