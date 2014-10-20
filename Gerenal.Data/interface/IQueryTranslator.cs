using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace General.Data
{
    public interface IQueryTranslator
    {
        Dictionary<string, object> Parameters { get; set; }
        string ToWhere();
        void Translate(Expression expression);
        bool UseTableAlias { get; set; }
    }
}
