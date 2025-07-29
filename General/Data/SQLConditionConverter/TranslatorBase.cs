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
        protected DbProvider _dbcategories;
        protected bool TraceMode;
        protected List<string> textCondition = new List<string>();
        protected QueryParameters queryParameters = new QueryParameters();
        protected List<Expression<Func<ConditonEntity, bool>>> expressions = new List<Expression<Func<ConditonEntity, bool>>>();
        protected Dictionary<string, string> _conditonColMapping = new Dictionary<string, string>();
        protected bool _isSkipNullOrEmtpybyAll = false;
        protected TranslatorBase()
        {
            
        }
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            this.traceOutput($"Method name {m.Method.Name}");
            //Like
            if (m.Arguments.Count == 1 && (m.Method.Name == "Contains" || m.Method.Name == "StartsWith" || m.Method.Name == "EndsWith"))
            {
                var columnExpression = m.Object as MemberExpression;
                if (columnExpression != null && (m.Arguments[0].NodeType == ExpressionType.MemberAccess
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
                    sb.Append($"{findColumnName(columnExpression)} LIKE {paramName}");
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
                //columnExpression.Member.GetType().IsArray
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
                        int index = 0;
                        StringBuilder paramNames = new StringBuilder();
                        foreach (var oitem in arrValue)
                        {
                            if (arrValue.Count() < 2100)
                            {
                                var paramName = $"{GetParameterSymbol()}{this._paraName}{this._paraCount++}";
                                var parameter = new QueryParameter();
                                //mapping dbtype
                                parameter.DbType = ConvertTypeCodeToDbType(Type.GetTypeCode(oitem.GetType()), oitem);
                                //if (oitem.GetType().IsEnum)
                                //{
                                //    parameter.Value = Convert.ToInt32(oitem);
                                //}
                                //else
                                //{
                                //    parameter.Value = oitem;
                                //}
                                parameter.Value = oitem;
                                parameter.Name = paramName;
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
                            if (index != (arrValue).Count() - 1)
                            {
                                paramNames.Append(",");
                            }
                            index++;
                        }
                        sb.Append("(");
                        sb.Append($"{findColumnName(columnExpression)} IN ({paramNames.ToString()})");
                        sb.Append(")");
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
                        if (IsNullConstant(b.Right))
                        {
                            tsb.Append(" IS ");
                        }
                        else
                        {
                            tsb.Append(" = ");
                        }
                        break;

                    case ExpressionType.NotEqual:
                        if (IsNullConstant(b.Right))
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
                this.traceOutput($"find field {_fieldName}");
                tsb.Append(_fieldName);
                return m;
            }
            else
            {
                this.setVariable(_fieldName, getMemberValue(m));
                //sb.Append(getMemberValue(m));
                return m;
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
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

        protected bool IsNullConstant(Expression exp)
        {
            return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
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
            if ((value != null && (Type.GetTypeCode(value.GetType()) == TypeCode.String && !string.IsNullOrEmpty(value.ToString()))) || !_isSkipNullOrEmtpybyAll)
            {
                QueryParameter parameter = new QueryParameter();
                //mapping dbtype
                var paramName = $"{GetParameterSymbol()}{this._paraName}{this._paraCount++}";
                tsb.Append($"{paramName}");
                parameter.DbType = ConvertTypeCodeToDbType(Type.GetTypeCode(value.GetType()), value);
                parameter.Value = value;
                parameter.Name = paramName;
                this.queryParameters.Add(parameter);
                //tsb.Clear();
            }
            else
            {
                tsb.Clear();
            }
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
                if (b.Left.NodeType == ExpressionType.Constant)
                {
                    var member = b.Right as MemberExpression;
                    if (b.Right.NodeType == ExpressionType.MemberAccess && member != null)
                    {
                        return member.Member.DeclaringType == typeof(ConditonEntity);
                    }

                }

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
            bool retCode;
            var charArray = value.ToCharArray();
            byte[] bytes = new byte[charArray.Length];
            for (int i = 0; i < charArray.Length; i++)
            {
                bytes[i] = (byte)charArray[i];
            }
            retCode = string.Equals(encoding.GetString(bytes, 0, bytes.Length), value, StringComparison.InvariantCulture);
            return retCode;
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
