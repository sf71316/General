using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gerneral.Helper
{
    public static class ValidateionHelper
    {
        public static Validation Begin( List<string> list)
        {
            return new Validation() { _list=list };
        }
    }
}
