using General.Data.SQLConditionConverter.Enum;
using General.Data.SQLConditionConverter.Extensions;
using General.Data.SQLConditionConverter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace General.Data.SQLConditionConverter
{
    public class QueryConditionTranslator<ConditonEntity> : TranslatorBase<ConditonEntity>, IQueryConditionExtractor where ConditonEntity : class, new()
    {
        public QueryParameters Parameters { get { return queryParameters; } }


        public QueryConditionTranslator(bool endableTraceMode = false)
        {
            TraceMode = endableTraceMode;
            this._dbcategories = DbProvider.MSSQL;
        }
        public QueryConditionTranslator<ConditonEntity> AddCondition(Expression<Func<ConditonEntity, bool>> conditon)
        {
            expressions.Add(conditon);
            return this;
        }
        public QueryConditionTranslator<ConditonEntity> AddTextCondition(string conditon)
        {
            textCondition.Add(conditon);
            return this;
        }
        public QueryConditionTranslator<ConditonEntity> AddTextCondition(string conditon, params QueryParameter[] parameters)
        {
            textCondition.Add(conditon);
            queryParameters.AddRange(parameters);
            return this;
        }
        public string Translate()
        {
            initInterceptor();
            //dynamic condition
            foreach (var eitem in this.expressions)
            {
                if (this.TraceMode)
                {
                    this.traceOutput(eitem.Simplify().ToString());
                }
                sb.Append("(");
                this.Visit(eitem);
                sb.Append(")");
                if (eitem != this.expressions.Last())
                {
                    sb.Append(" AND ");
                }
            }

            //text condition
            if (this.textCondition.Count > 0)
            {
                sb.Append(" AND ");
                this.sb.Append($"{(this.sb.Length > 0 ? " AND " : "")}" + string.Join("AND", this.textCondition.Select(p => $"({p})")));
            }
            return this.sb.ToString();
        }

        private void initInterceptor()
        {
            var attr = typeof(ConditonEntity).GetCustomAttributes(true).FirstOrDefault(x => x is IConditionMappingAttribute<ConditonEntity>);
            if (attr != null)
            {
                var interceptor = attr as IConditionMappingAttribute<ConditonEntity>;
                interceptor.Mapping();
                this._isSkipNullOrEmtpybyAll = interceptor.IsSkipNullOrEmpty;
                foreach (KeyValuePair<Expression<Func<ConditonEntity, object>>, string> m in interceptor.MappingCollection)
                {
                    if (m.Key.Body is MemberExpression)
                    {
                        var prop = (MemberExpression)m.Key.Body;
                        this._conditonColMapping.Add(prop.Member.Name, m.Value);
                    }
                    else if (m.Key.Body is UnaryExpression)
                    {
                        var u = (UnaryExpression)m.Key.Body;
                        var prop = u.Operand as MemberExpression;
                        this._conditonColMapping.Add(prop.Member.Name, m.Value);

                    }
                }
            }
        }
    }
}
