using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;

namespace General.Data.SQLConditionConverter.Interfaces
{
    public interface IConditionMappingAttribute<ConditionParameter>  where ConditionParameter : class, new()
    {
        Dictionary<Expression<Func<ConditionParameter, object>>, string> MappingCollection { get; set; }
        bool IsSkipNullOrEmpty { get; set; }
        void Mapping();
    }
}
