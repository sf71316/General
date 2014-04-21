using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    internal class EnumConverter : ITypeConverter
    {
        public object Convert(object ValueToConvert)
        {
            //TODO Convert(object ValueToConvert)未實作
            throw new NotImplementedException();
        }

        public object Convert(Type EnumType, object ValueToConvert)
        {
            if (!EnumType.IsEnum)
                throw new InvalidOperationException("ERROR_TYPE_IS_NOT_ENUMERATION");

            return System.Convert.ChangeType(Enum.Parse(EnumType, ValueToConvert.ToString()), EnumType);
        }
    }
}
