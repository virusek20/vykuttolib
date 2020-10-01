using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace vykuttolib.Services.StaticFiles
{
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://tempuri.org/MimeType.xsd")]
    [XmlRoot(Namespace = "http://tempuri.org/MimeType.xsd", IsNullable = false)]
    public class AllowedMimeTypes
    {
        [XmlElement("MimeType")]
        public AllowedMimeType[] MimeType { get; set; }
    }

    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://tempuri.org/MimeType.xsd")]
    public partial class AllowedMimeType
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        [XmlAttribute("Offset")]
        public int Offset { get; set; }

        [XmlElement("Signature")]
        public string[] Signatures { get; set; }

        public IEnumerable<byte[]> BinarySignatures => Signatures.Select(s => Convert.FromBase64String(s));
    }
}
