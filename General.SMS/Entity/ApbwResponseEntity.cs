using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace General.SMS
{
    [XmlRoot(ElementName = "Response", Namespace = "")]
    public class ApbwResponseEntity : IResponse
    {
         [XmlElement("MDN")]
        public string MDN { get; set; }
        [XmlElement("RtnDateTime")]
        public string RtnDateTime { get; set; }
        [XmlElement("TaskID")]
        public string TaskID { get; set; }
        [XmlElement("Code")]
        public string Code { get; set; }
        [XmlElement("Reason")]
        public string Reason { get; set; }
    }
}
