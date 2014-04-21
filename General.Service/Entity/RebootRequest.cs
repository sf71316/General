using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace General.Service
{
    public class RebootRequest
    {
        /// <summary>
        /// Mac Address
        /// </summary>
        public string DeviceExternalID { get; set; }

        public string Message { get; set; }

        public string Reson { get; set; }

        public long Timeout { get; set; }

        public bool ForceReboot { get; set; }

        public XmlElement[] Any { get; set; } 
    }
}
