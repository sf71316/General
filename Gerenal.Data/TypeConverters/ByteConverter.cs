using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace General.Data
{
    internal class ByteConverter : ITypeConverter
    {
        public object Convert(object ValueToConvert)
        {

            if (ValueToConvert == null || ValueToConvert == DBNull.Value)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, ValueToConvert);
            return ms.ToArray();
        }
    }
}
