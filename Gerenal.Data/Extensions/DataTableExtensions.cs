using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace General.Data
{
    public  static class DataTableExtensions
    {
        public static IEnumerable<T> Fille<T>(this DataTable dt) where T:class,new()
        {
            List<T> list = new List<T>();
            foreach (DataRow item in dt.Rows)
            {
                T t = new T();
                item.Fill(t);
                list.Add(t);
            }
            return list;
        }
    }
}
