using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace General.Data.Dapper
{
    internal class DapperUpdateCommandBuilder : DapperCommandBuilder, IUpdateCommand
    {
        public DapperUpdateCommandBuilder(string tablename, IDapperProvider dapper)
            : base(tablename, dapper)
        {

        }
        public int Update(object e, Expression expr)
        {
            DynamicParameters paramer = new DynamicParameters();
            DbTypeConverter tconvert = new DbTypeConverter();

            string queryPattern = "UPDATE {0} SET {1} {2} ";
            this.Translator.Translate(expr);
            string condition = this.Translator.ToWhere();
            foreach (var item in this.Translator.Parameters)
            {
                paramer.Add(item.Key, item.Value, tconvert.Get(item.Value.GetType()), null, null);
            }
            if (!string.IsNullOrEmpty(condition))
            {
                PropertyInfo[] props = e.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                List<string> fieldList = new List<string>();
                foreach (PropertyInfo prop in props)
                {
                    if (prop.IsDefined(typeof(ColumnAttribute), true))
                    {
                        ColumnAttribute attr = prop.GetInstancetAttribute<ColumnAttribute>();

                        if (attr.Ignore || attr.AutoKey)
                            continue;
                        //object v = prop.GetValue(e, null);
                        object v = prop.GetValueGetter<object>();
                        if (v != null)
                        {
                            string fieldName = !String.IsNullOrEmpty(attr.FieldName) ? attr.FieldName : prop.Name;
                            string parameterName = String.Format("{0}_{1}", this.TableName, fieldName);

                            fieldList.Add(String.Format("{0} = @{1}", fieldName, parameterName));
                            paramer.Add(fieldName, v, tconvert.Get(v.GetType()), null, null);

                        }
                    }
                }
                string query =
                      String.Format(queryPattern,
                      this.TableName,
                      String.Join(", ", fieldList.ToArray()),
                      condition);
                return this.Dapper.Execute(query, paramer);
            }
            else
            {
                this.OnException("Not set Condition");
                return 0;
            }
        }


        public int Update<T>(object e,Expression<Func<T, bool>> expr)
        {
            return this.Update<T>(e,expr);
        }
    }
}
