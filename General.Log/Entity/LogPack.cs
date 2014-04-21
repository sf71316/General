using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Log
{
    public class LogPack
    {
        public string UserName { get; set; }
        public Type ClassType { get; set; }
        public DateTime LogTime { get; set; }
        public string IP { get; set; }
    }
}
