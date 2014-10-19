using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace General.Data
{
    interface IQueryTranslator
    {
        IDictionary<string, object> Parameters { get; set; }
        string ToWhere();
        void Translate(Expression expression);
        bool UseTableAlias { get; set; }
    }
}
