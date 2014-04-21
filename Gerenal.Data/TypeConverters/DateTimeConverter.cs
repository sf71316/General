using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    internal class DateTimeConverter : ITypeConverter
    {
        public object Convert(object ValueToConvert)
        {
            if (ValueToConvert == null || ValueToConvert == DBNull.Value)
                return DateTime.MinValue;
            return System.Convert.ToDateTime(ValueToConvert);
        }
    }
}
