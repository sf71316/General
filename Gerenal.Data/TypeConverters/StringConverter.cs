using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    internal class StringConverter : ITypeConverter
    {
        public object Convert(object ValueToConvert)
        {
            if (ValueToConvert == null || ValueToConvert == DBNull.Value)
                return string.Empty;

            return ValueToConvert.ToString();
        }
    }
}
