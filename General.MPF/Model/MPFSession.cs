using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.MPF
{
    public class MPFSession:Dictionary<string,object>
    {
        public object this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = value;
            }
        }
    }
    public class UserSession : Dictionary<string, object>
    {

    }
}
