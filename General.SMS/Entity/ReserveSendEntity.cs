using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace General.SMS
{
    [XmlRoot(ElementName = "Request")]
    public class ReserveSendEntity : IRequest
    {
        public ReserveSendEntity()
        {
            this.PhoneList = new List<string>();
            this.MDN = "0982719051";
            this.UID = "emax";
            this.UPASS = "sms24076789";
        }

        [XmlElement("Subject")]
        public string Subject { get; set; }
        [XmlElement("Retry")]
        public string Retry { get; set; }
        [XmlElement("AutoSplite")]
        public string AutoSplite { get; set; }
        [XmlElement("StartDateTime")]
        public string StartDateTime { get; set; }
        [XmlElement("StopDateTime")]
        public string StopDateTime { get; set; }
        public string MDN { get; set; }
        [XmlElement("Message")]
        public string Message { get; set; }
        public string UID { get; set; }
        public  string UPASS { get; set; }
        [XmlArray("MDNList")]
        [XmlArrayItem("MSISDN")]
        public List<string> PhoneList { get; set; }
    }
}
