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

    }
    public class QueryParameters : List<QueryParameter>
    {

    }
}
