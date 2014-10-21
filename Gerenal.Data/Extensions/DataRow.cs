using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace General.Data
{
    public static class DataRowExtensions
    {
        public static void Fill(this DataRow dr, object e)
        {
            
            PropertyInfo[] pre = e.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo item in pre)
            {
                string _name = item.Name.ToLower();
                if (dr != null)
                {
                    if (dr.Table.Columns.Contains(_name))
                    {
                        if (dr[_name] != null)
                        {
                            ExpressionFactory.Set(e, dr[_name], _name);
                            //item.SetValue(e, dr[_name], null);
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}
