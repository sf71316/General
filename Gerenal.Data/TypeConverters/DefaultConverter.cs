using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace General.Data
{
    internal class DefaultConverter: ITypeConverter
    {
        public object Convert(object ValueToConvert)
        {
            
          //  if (ValueToConvert == null || ValueToConvert == DBNull.Value)
         //       return DBNull.Value;
            return ValueToConvert;
        }
    }
}
