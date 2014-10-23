using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using System.Reflection;
using System.Data;

namespace General.Data.Dapper
{
    internal class DapperInsertCommandBuilder:DapperCommandBuilder,IInsertCommand
    {
        public DapperInsertCommandBuilder(IDapperProvider dapper)
            : base( dapper)
        {

        }
        public object Insert(object e)
        {
            DynamicParameters paramer = new DynamicParameters();
            DbTypeConverter tconvert=new DbTypeConverter();
            string queryPattern = "INSERT INTO {0} ({1}) VALUES({2})";
            PropertyInfo[] props = e.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            List<string> fieldList = new List<string>();
            List<string> valueList = new List<string>();
            foreach (PropertyInfo prop in props)
            {
                string fieldName = prop.Name;
                ColumnAttribute attr = prop.GetInstancetAttribute<ColumnAttribute>();
                if (attr != null)
                {
                    fieldName = !string.IsNullOrEmpty(attr.Name) ? attr.Name : fieldName;
                    if (attr.AutoKey || attr.Ignore)
                        continue;
                }
                object v = prop.GetValue(e, null);
                if (v != null||(attr!=null && attr.IsNull))
                {
                    string parameterName = string.Format("{0}_{1}", this.TableName, fieldName);
                    fieldList.Add(string.Format("[{0}]",fieldName));
                    valueList.Add(string.Format("@{0}", parameterName));
                    paramer.Add(parameterName, v, tconvert.Get(v.GetType()), null, null);
                }
            }
            string query =
                string.Format(queryPattern,
                this.TableName,
                string.Join(", ", fieldList.ToArray()),
                string.Join(", ", valueList.ToArray()));
            return this.Dapper.Execute(query, paramer, CommandType.Text);
        }
    }
}
