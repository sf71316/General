using General.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.MPF
{
    public class Account:IEntity
    {
        public string SubsID { get; set; }
        public string ServiceName { get; set; }
        public string SmartCard { get; set; }
    }
}
