﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    internal class BooleanConverter : ITypeConverter
    {
        public object Convert(object ValueToConvert)
        {
            if (ValueToConvert == null || ValueToConvert == DBNull.Value)
                return false;

            if (string.IsNullOrEmpty(ValueToConvert.ToString()))
                return false;
            else if (ValueToConvert.ToString() == "0")
                return false;
            else if (ValueToConvert.ToString() == "1")
                return true;
            else
                return System.Convert.ToBoolean(ValueToConvert);
        }
    }
}
