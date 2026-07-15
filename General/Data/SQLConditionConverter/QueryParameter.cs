using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace General.Data.SQLConditionConverter
{
    public class QueryParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public DbType DbType { get; set; }
        /// <summary>
        /// 參數長度。0 表示未指定，由 ADO.NET provider 依 DbType 與實際值自行推導。
        /// </summary>
        public int Size { get; set; }

    }
    public class QueryParameters : List<QueryParameter>
    {

    }
}
