using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gerneral.Helper
{
    public static partial class ValidationExtensions
    {
        public static Validation Check(this Validation validation, Func<bool> method, string error)
        {
            if (!method())
            {
                if( validation._list!=null)
                    validation._list.Add(error);
            }
                return validation;
        }
    }
}
