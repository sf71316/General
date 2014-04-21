using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    internal class CharConverter : ITypeConverter
    {
        public object Convert(object ValueToConvert)
        {
            if (ValueToConvert == null || ValueToConvert == DBNull.Value)
                return (char)0x0;

            return System.Convert.ToChar(ValueToConvert);
        }
    }
}
