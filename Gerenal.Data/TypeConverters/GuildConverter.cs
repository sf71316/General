using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    class GuildConverter : ITypeConverter
    {
        public object Convert(object ValueToConvert)
        {
            if (ValueToConvert == null || ValueToConvert == DBNull.Value)
                return Guid.Empty;

            return new Guid(ValueToConvert.ToString());
        }
    }
}
