using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
   internal sealed class NullableConverter:ITypeConverter
    {
        public object Convert(object ValueToConvert)
        {
            if (ValueToConvert == null || ValueToConvert == DBNull.Value)
                return null;
            Type innerType = ValueToConvert.GetType();
            return System.Convert.ChangeType(ValueToConvert, innerType);                
        }
    }
}
