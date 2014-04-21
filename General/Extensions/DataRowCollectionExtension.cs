using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace General
{
    public static class DataRowCollectionExtension
    {
        public static List<TOutput> SpliteString<TOutput>(this DataRowCollection rows, Converter<DataRow, TOutput> func)
        {
            List<TOutput> list = new List<TOutput>();
            foreach (DataRow item in rows)
            {
                list.Add(func.Invoke(item));
            }
            return list;
        }
    }
}
