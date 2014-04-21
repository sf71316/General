using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    public interface ITypeConverter
    {
        object Convert(object ValueToConvert);
    }
}
