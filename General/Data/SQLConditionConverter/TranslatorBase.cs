using General.Data.SQLConditionConverter.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Text;

namespace General.Data.SQLConditionConverter
{
    public abstract class TranslatorBase<ConditonEntity> : ExpressionVisitor where ConditonEntity : class, new()
    {
        private readonly string _paraName = "c";
        private int _paraCount = 1;
        protected StringBuilder sb = new StringBuilder();
        protected StringBuilder tsb = new StringBuilder();
        protected bool _isfield = false;
        protected string _fieldName = string.Empty;
        protected int _fieldLength = 0;
        protected DbProvider _dbcategories;
        protected bool TraceMode;
        protected List<string> textCondition = new List<string>();
        protected QueryParameters queryParameters = new QueryParameters();
        protected QueryParameters textConditionParameters = new QueryParameters();
        protected List<Expression<Func<ConditonEntity, bool>>> expressions = new List<Expression<Func<ConditonEntity, bool>>>();
        protected Dictionary<string, string> _conditonColMapping = new Dictionary<string, string>();
        protected bool _isSkipNullOrEmtpybyAll = false;
        protected TranslatorBase()
        {

        }
        protected void Reset()
        {
            sb.Clear();
            tsb.Clear();
            _paraCount = 1;
            _isfield = false;
            _fieldName = string.Empty;
            _fieldLength = 0;
            queryParameters.Clear();
            _conditonColMapping.Clear();
            _isSkipNullOrEmtpybyAll = false;
        }
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            this.traceOutput($"Method name {m.Method.Name}");
            //Like or instance Contains (e.g. List<T>.Contains)
            if (m.Arguments.Count == 1 && (m.Method.Name == "Contains" || m.Method.Name == "StartsWith" || m.Method.Name == "EndsWith"))
            {
                // Instance Contains on a collection: list.Contains(p.Field) → IN
                if (m.Method.Name == "Contains" && m.Object != null && isEntityMember(m.Arguments[0]))
                {
                    var columnExpression = getEntityMember(m.Arguments[0]);
                    if (columnExpression != null)
                    {
                        object objValue = getMemberValue(m.Object as MemberExpression ?? (MemberExpression)((UnaryExpression)m.Object).Operand);
                        if (objValue == null) { objValue = Expression.Lambda(m.Object).Compile().DynamicInvoke(); }
                        IEnumerable<object> arrValue = null;
                        if (objValue is IEnumerable enumerable)
                        {
                            arrValue = enumerable.OfType<object>();
                        }
                        if (arrValue != null)
                        {
                            buildInClause(columnExpression, arrValue);
                        }
                        return m;
                    }
                }

                // String LIKE: p.Field.Contains/StartsWith/EndsWith("value")
                var likeColumnExpression = m.Object as MemberExpression;
                if (likeColumnExpression != null && (m.Arguments[0].NodeType == ExpressionType.MemberAccess
                    || m.Arguments[0].NodeType == ExpressionType.Constant))
                {
                    object cValue = null;
                    if (m.Arguments[0].NodeType == ExpressionType.MemberAccess)
                    {
                        var valueExpression = m.Arguments[0] as MemberExpression;
                        cValue = getMemberValue(valueExpression);
                    }
                    else
                    {
                        var valueExpression = m.Arguments[0] as ConstantExpression;
                        cValue = valueExpression.Value;
                    }

                    var paramName = $"{GetParameterSymbol()}{this._paraName}{this._paraCount++}";
                    sb.Append("(");
                    sb.Append($"{findColumnName(likeColumnExpression)} LIKE {paramName}");
                    QueryParameter parameter = new QueryParameter();
                    //mapping dbtype
                    parameter.DbType = ConvertTypeCodeToDbType(Type.GetTypeCode(cValue.GetType()), cValue);
                    if (m.Method.Name == "Contains")
                    {
                        parameter.Value = "%" + cValue + "%";
                    }
                    else if (m.Method.Name == "StartsWith")
                    {
                        parameter.Value = cValue + "%";
                    }
                    else
                    {
                        parameter.Value = "%" + cValue;
                    }
                    parameter.Name = paramName;
                    this.queryParameters.Add(parameter);
                    sb.Append(")");
                }
                return m;
            }
            else if (m.Arguments.Count == 2 && (m.Method.Name == "Contains"))
            {
                var valueExpression = m.Arguments[0] as MemberExpression;
                var columnExpression = m.Arguments[1] as MemberExpression;
                if (columnExpression != null)
                {
                    IEnumerable<object> arrValue = null;
                    if (valueExpression != null)
                    {
                        var objValue = getMemberValue(valueExpression);
                        if (objValue.GetType().IsArray && ((Array)objValue)?.Length > 0)
                        {
                            arrValue = ((Array)objValue).OfType<object>();
                        }
                        else if (objValue is IEnumerable)
                        {
                            arrValue = ((IEnumerable)objValue).OfType<object>();
                        }
                    }
                    else if (m.Arguments[0].NodeType == ExpressionType.NewArrayInit)
                    {
                        var arrvalueExpression = m.Arguments[0] as NewArrayExpression;
                        arrValue = Enumerable.ToArray(arrvalueExpression.Expressions.Select(p => p as ConstantExpression).Select(v => v.Value));
                    }
                    if (arrValue != null)
                    {
                        buildInClause(columnExpression, arrValue);
                    }
                }
                return m;
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }
        protected override Expression VisitUnary(UnaryExpression u)
        {
            this.traceOutput($"Unary {u.NodeType}");
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    tsb.Append(" NOT ");
                    this.Visit(u.Operand);
                    break;
                case ExpressionType.Convert:
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }
        protected override Expression VisitBinary(BinaryExpression b)
        {
            var islrswitch = switchRL(b);
            _isfield = false;
            _fieldName = string.Empty;
            _fieldLength = 0;
            //tsb.Append("(");
           
            if (islrswitch)
            {
                this.Visit(b.Right);
            }
            else
            {

                this.Visit(b.Left);
            }

            if (sb.Length > 0 || tsb.Length > 0)
            {
                switch (b.NodeType)
                {
                    case ExpressionType.And:
                        tsb.Append(" AND ");
                        break;

                    case ExpressionType.AndAlso:
                        tsb.Append(" AND ");
                        break;

                    case ExpressionType.Or:
                        tsb.Append(" OR ");
                        break;

                    case ExpressionType.OrElse:
                        tsb.Append(" OR ");
                        break;

                    case ExpressionType.Equal:
                        if (IsNullConstant(islrswitch ? b.Left : b.Right))
                        {
                            tsb.Append(" IS ");
                        }
                        else
                        {
                            tsb.Append(" = ");
                        }
                        break;

                    case ExpressionType.NotEqual:
                        if (IsNullConstant(islrswitch ? b.Left : b.Right))
                        {
                            tsb.Append(" IS NOT ");
                        }
                        else
                        {
                            tsb.Append(" <> ");
                        }
                        break;

                    case ExpressionType.LessThan:
                        tsb.Append(" < ");
                        break;

                    case ExpressionType.LessThanOrEqual:
                        tsb.Append(" <= ");
                        break;

                    case ExpressionType.GreaterThan:
                        tsb.Append(" > ");
                        break;

                    case ExpressionType.GreaterThanOrEqual:
                        tsb.Append(" >= ");
                        break;

                    default:
                        throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));

                }
            }
            if (islrswitch)
            {
                this.Visit(b.Left);
            }
            else
            {
                this.Visit(b.Right);
            }
            if (tsb.Length > 0)
            {
                sb.Append(tsb.ToString());
                tsb.Clear();

            }
            if (sb.Length > 0)
            {
                //if (sb.ToString().LastOrDefault() != ')')
                //    sb.Append(")");
            }


            return b;
        }
        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;

            if (q == null && c.Value == null)
            {
                //tsb.Append(tsb.ToString());
                tsb.Append("NULL");
                //tsb.Clear();
            }
            else if (q == null)
            {
                this.setVariable(_fieldName, c.Value);
            }

            return c;
        }
        protected override Expression VisitMember(MemberExpression m)
        {
            //如果成員非泛型指定型別則嘗試取得數值
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter && typeof(ConditonEntity) == m.Member.DeclaringType)
            {
                _isfield = true;
                _fieldName = findColumnName(m);
                _fieldLength = findColumnLength(m);
                this.traceOutput($"find field {_fieldName}");
                tsb.Append(_fieldName);
                return m;
            }
            else
            {
                this.setVariable(_fieldName, getMemberValue(m));
                return m;
            }
        }
        private string findColumnName(MemberExpression m)
        {
            var attr = m.Member.GetCustomAttributes(true).FirstOrDefault(x => x is ColumnMappingAttribute);
            if (this._conditonColMapping.ContainsKey(m.Member.Name))
            {
                var value = "";
                this._conditonColMapping.TryGetValue(m.Member.Name, out value);
                return value;
            }
            else if (attr != null)
            {
                var cattr = attr as ColumnMappingAttribute;
                return $"{(string.IsNullOrEmpty(cattr.AliasName) ? string.Empty : cattr.AliasName + ".")}{cattr.ColumnName}";

            }
            return m.Member.Name;
        }
        private int findColumnLength(MemberExpression m)
        {
            var attr = m.Member.GetCustomAttributes(true).FirstOrDefault(x => x is ColumnMappingAttribute) as ColumnMappingAttribute;
            return attr?.Length ?? 0;
        }

        protected bool IsNullConstant(Expression exp)
        {
            return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
        }
        private MemberExpression getEntityMember(Expression expr)
        {
            if (expr is MemberExpression member && isEntityMember(member))
                return member;
            if (expr is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
                return getEntityMember(unary.Operand);
            return null;
        }
        private void buildInClause(MemberExpression columnExpression, IEnumerable<object> arrValue)
        {
            var arrList = arrValue as IList<object> ?? arrValue.ToList();
            if (arrList.Count == 0)
            {
                sb.Append("(1=0)");
            }
            else
            {
                bool useParams = arrList.Count < 2100;
                int columnLength = findColumnLength(columnExpression);
                StringBuilder paramNames = new StringBuilder();
                for (int i = 0; i < arrList.Count; i++)
                {
                    var oitem = arrList[i];
                    if (useParams)
                    {
                        var paramName = $"{GetParameterSymbol()}{this._paraName}{this._paraCount++}";
                        var parameter = new QueryParameter();
                        parameter.DbType = ConvertTypeCodeToDbType(Type.GetTypeCode(oitem.GetType()), oitem);
                        parameter.Value = oitem;
                        parameter.Name = paramName;
                        parameter.Size = columnLength;
                        this.queryParameters.Add(parameter);
                        paramNames.Append(paramName);
                    }
                    else
                    {
                        if (IsNumber(oitem))
                        {
                            paramNames.Append(oitem);
                        }
                        else
                        {
                            paramNames.Append($"'{oitem}'");
                        }
                    }
                    if (i < arrList.Count - 1)
                    {
                        paramNames.Append(",");
                    }
                }
                sb.Append("(");
                sb.Append($"{findColumnName(columnExpression)} IN ({paramNames.ToString()})");
                sb.Append(")");
            }
        }
        protected void traceOutput(string message)
        {
            if (this.TraceMode)
            {
                Debug.WriteLine(message);
            }
        }
        private void setVariable(string fieldName, object value)
        {
            if (_isSkipNullOrEmtpybyAll)
            {
                // 跳過 null
                if (value == null)
                {
                    tsb.Clear();
                    return;
                }
                // 跳過空字串
                if (Type.GetTypeCode(value.GetType()) == TypeCode.String && string.IsNullOrEmpty(value.ToString()))
                {
                    tsb.Clear();
                    return;
                }
            }

            if (value == null)
            {
                tsb.Append("NULL");
                return;
            }

            QueryParameter parameter = new QueryParameter();
            var paramName = $"{GetParameterSymbol()}{this._paraName}{this._paraCount++}";
            tsb.Append($"{paramName}");
            parameter.DbType = ConvertTypeCodeToDbType(Type.GetTypeCode(value.GetType()), value);
            parameter.Value = value;
            parameter.Name = paramName;
            parameter.Size = _fieldLength;
            this.queryParameters.Add(parameter);
        }
        private object getMemberValue(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }
        private Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }
        private string GetParameterSymbol()
        {
            switch (this._dbcategories)
            {
                case DbProvider.MSSQL:
                    return "@";
                case DbProvider.Oracel:
                    return "?";
                case DbProvider.MySql:
                    return "?";
                default:
                    break;
            }
            return "";
        }
        private bool switchRL(BinaryExpression b)
        {
            if (b.Left != null && b.Right != null)
            {
                // 左邊不是欄位存取，右邊是欄位存取 → 需要交換
                bool leftIsField = isEntityMember(b.Left);
                bool rightIsField = isEntityMember(b.Right);
                if (!leftIsField && rightIsField)
                {
                    return true;
                }
            }
            return false;
        }
        private bool isEntityMember(Expression expr)
        {
            if (expr is MemberExpression member)
            {
                return member.Expression != null
                    && member.Expression.NodeType == ExpressionType.Parameter
                    && member.Member.DeclaringType == typeof(ConditonEntity);
            }
            if (expr is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            {
                return isEntityMember(unary.Operand);
            }
            return false;
        }
        private DbType ConvertTypeCodeToDbType(TypeCode typeCode, object value = null)
        {

            // no TypeCode equivalent for TimeSpan or DateTimeOffset
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return DbType.Boolean;
                case TypeCode.Byte:
                    return DbType.Byte;
                case TypeCode.Char:
                    return DbType.StringFixedLength;    // ???
                case TypeCode.DateTime: // Used for Date, DateTime and DateTime2 DbTypes
                    return DbType.DateTime;
                case TypeCode.Decimal:
                    return DbType.Decimal;
                case TypeCode.Double:
                    return DbType.Double;
                case TypeCode.Int16:
                    return DbType.Int16;
                case TypeCode.Int32:
                    return DbType.Int32;
                case TypeCode.Int64:
                    return DbType.Int64;
                case TypeCode.SByte:
                    return DbType.SByte;
                case TypeCode.Single:
                    return DbType.Single;
                case TypeCode.String:
                    return (value != null && checkEncoding(value.ToString(), Encoding.UTF8)) ? DbType.String : DbType.AnsiString;
                case TypeCode.UInt16:
                    return DbType.UInt16;
                case TypeCode.UInt32:
                    return DbType.UInt32;
                case TypeCode.UInt64:
                    return DbType.UInt64;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                case TypeCode.Object:
                default:
                    return DbType.Object;
            }
        }
        private bool checkEncoding(string value, Encoding encoding)
        {
            // 判斷字串是否包含非 ASCII 字元（如中文），若有則回傳 true 表示需要 NVarChar (DbType.String)
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] > 127)
                {
                    return true;
                }
            }
            return false;
        }
        private static bool IsNumber(object obj)
        {
            if (Equals(obj, null))
            {
                return false;
            }

            Type objType = obj.GetType();
            objType = Nullable.GetUnderlyingType(objType) ?? objType;

            if (objType.IsPrimitive)
            {
                return objType != typeof(bool) &&
                    objType != typeof(char) &&
                    objType != typeof(IntPtr) &&
                    objType != typeof(UIntPtr);
            }

            return objType == typeof(decimal);
        }
    }
}
