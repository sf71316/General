using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gerneral.Helper
{
    public static partial class ValidationExtensions
    {
        public static Validation NotNull(this Validation validation, object obj)
        {
            return Check(validation, () =>{
                return  obj != null;
            }, "不可為空");
        }
    }
   
}
