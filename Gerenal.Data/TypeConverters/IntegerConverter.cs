using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    internal class IntegerConverter : ITypeConverter
    {
        public object Convert(object ValueToConvert) 
        {
            if (ValueToConvert == null || ValueToConvert == DBNull.Value)
                return 0;

            return System.Convert.ToInt32(ValueToConvert);
        }
    }
}
