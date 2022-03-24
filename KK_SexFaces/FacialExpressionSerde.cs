using System.IO;
using System.Xml.Serialization;

namespace SexFaces
{
    public partial class FacialExpression
    {
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
    }
}
