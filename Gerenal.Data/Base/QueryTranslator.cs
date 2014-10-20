using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace General.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    public class QueryTranslator : ExpressionVisitor,IQueryTranslator
    {

        private NodeDirect _direct;
        private Dictionary<string, object> _parameters;
        private IDictionary<string, string> _TablePrefix { get; set; }
        private Queue<QueryCondition> Conditions { get; set; }
        private int _TableCount = 0;
        private QueryCondition _condition;
        public bool UseTableAlias { get; set; }
        public QueryTranslator(bool useTableAlias = false)
        {
            _direct = NodeDirect.Left;
            this._parameters = new Dictionary<string, object>();
            this._TablePrefix = new Dictionary<string, string>();
            this.Conditions = new Queue<QueryCondition>();
            this.UseTableAlias = useTableAlias;
        }
        public void Translate(Expression expression)
        {
            this.Visit(expression);
        }
        //public void Translate<T>(Func<T,bool> func) where T:class
        //{
        //    this.Visit(Expression.Lambda<Func<T,bool>>(Expression.Call(func.Method)));
        //}
        public string ToWhere()
        {
            StringBuilder _sb = new StringBuilder();
            QueryCondition previous = null; ;
            foreach (var item in this.Conditions)
            {
                if (item.GetType() == typeof(QueryOptCondition))
                {
                    if (this._parameters.ContainsKey(previous.Parameter)
                        && !this.IsEmptyValue(this._parameters[previous.Parameter]))
                        _sb.AppendFormat("{0}", item.Operator.Trim());
                }
                else if (item.GetType() == typeof(QueryCondition))
                {
                    if (this._parameters.ContainsKey(item.Parameter)
                        && !this.IsEmptyValue(this._parameters[item.Parameter]))
                        _sb.AppendFormat("({0} {1} {2})", item.Field, item.Operator, item.Parameter);
                    else
                    {
                        if (previous != null && previous.GetType() == typeof(QueryOptCondition))
                            _sb.Append("1=1");
                    }
                }
                previous = item;
            }
            if (_sb.Length > 0)
            {
                return "WHERE " + _sb.ToString();
            }
            else
            {
                this._parameters.Clear();
                return string.Empty;
            }

        }
        public void Clear()
        {
            this._parameters.Clear();
            this.Conditions.Clear();
        }
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                this.Visit(m.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                return m;
            }

            else if (m.Method.Name == "Contains")
            {
                if (m.Arguments.Count == 2)
                {
                    if (m.Arguments[0].Type.IsArray)
                    {
                        if (this.ParseIn(m))
                        {
                            this.Conditions.Enqueue(this._condition);
                        }

                        return m;
                    }
                }
                else
                {
                    if (this.ParseLike(m))
                    {
                        this.Conditions.Enqueue(this._condition);
                    }

                    return m;
                }
            }
            else
            {
                if (this._direct == NodeDirect.Right)
                {
                    ConstantExpression nextExpression = Expression.Constant(Expression.Lambda(m).Compile().DynamicInvoke());
                    this.Visit(nextExpression);
                    return m;
                }
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }
        public Func<MemberInfo, string> AttributeConvertHandler;
        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    //this.Visit(u.Operand);
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
            this._direct = NodeDirect.Left;
            this._condition = new QueryCondition();
            this.Visit(b.Left);
            if (!IsNodeOpt(b.Right.NodeType) &&
                b.Left.NodeType != ExpressionType.Call &&
                b.Right.NodeType != ExpressionType.Call)
            {
                this._condition.Operator = this.GetOperator(b);
                _direct = NodeDirect.Right;
                this.Visit(b.Right);
                this.Conditions.Enqueue(this._condition);
                return b;
            }
            else
            {
                this._condition = new QueryOptCondition();
                this._condition.Operator = this.GetOperator(b);
                this.Conditions.Enqueue(this._condition);
                this.Visit(b.Right);
                return b;
            }
        }
        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;

            if (q == null && c.Value == null)
            {
                //_sb.Append("NULL");
            }
            else if (q == null)
            {
                this._parameters.Add(this._condition.Parameter, c.Value);
            }

            return c;
        }
        protected override Expression VisitMember(MemberExpression m)
        {
            if (this._direct == NodeDirect.Left)
            {
                if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
                {

                    this._condition.Field = GetTableField(m);
                    this._condition.Parameter = GetParameterName(m.Expression.ToString(), m.Member.Name);
                    return m;
                }
            }
            if (this._direct == NodeDirect.Right) //當右節點非常數運算式(string,int....etc)
            {
                if (m.Expression != null &&
                    m.NodeType == ExpressionType.MemberAccess ||
                    m.Expression.NodeType == ExpressionType.Parameter)
                {
                    return this.VisitConstant(Expression.Constant(this.GetValue(m)));
                }
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
        private Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }
        private bool ParseIn(MethodCallExpression m)
        {
            List<string> _parameter = new List<string>();
            this._condition = new QueryCondition();
            var _field = m.Arguments[1] as MemberExpression;
            var _values = GetValue(m.Arguments[0] as MemberExpression) as IEnumerable;
            this._condition.Field = this.GetTableField(_field);
            this._condition.Operator = "IN";
            if (_values != null)
            {
                foreach (var item in _values)
                {
                    string _name = GetParameterName(_field.Expression.ToString(), _field.Member.Name);
                    _parameter.Add(_name);
                    this._parameters.Add(_name, item);
                }
                this._condition.Parameter = string.Format("({0})", string.Join(",", _parameter.ToArray()));
                return true;
            }
            return true;
        }
        private bool ParseLike(MethodCallExpression expression)
        {
            this._condition = new QueryCondition();
            object parameter = null;
            if (expression.Arguments[0].NodeType == ExpressionType.MemberAccess)
            {
                parameter = GetValue(expression.Arguments[0] as MemberExpression);
            }
            else
            {
                parameter = ((ConstantExpression)expression.Arguments[0]).Value;
            }
            var property = expression.Object as MemberExpression;
            this._condition.Field = property.Member.Name;
            this._condition.Operator = "LIKE";
            this._condition.Parameter = this.GetParameterName(property.Expression.ToString(), property.Member.Name);
            if (parameter != null)
            {
                var value = Expression.Constant((string.IsNullOrEmpty(parameter.ToString())) ? string.Empty : string.Format("'%{0}%'", parameter));
                this.Visit(value);
                return true;
            }
            return true;
        }
        private bool IsNodeOpt(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    return true;
                default:
                    return false;
            }
        }
        protected string GetOperator(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return " OR ";
                case ExpressionType.Equal:
                    if (IsNullConstant(b.Right))
                    {
                        return " IS ";
                    }
                    else
                    {
                        return " = ";
                    }
                case ExpressionType.NotEqual:
                    if (IsNullConstant(b.Right))
                    {
                        return " IS NOT ";
                    }
                    else
                    {
                        return " <> ";
                    }
                case ExpressionType.LessThan:
                    return " < ";
                case ExpressionType.LessThanOrEqual:
                    return " <= ";
                case ExpressionType.GreaterThan:
                    return " > ";
                case ExpressionType.GreaterThanOrEqual:
                    return " >= ";
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
        }
        private string GetTableField(MemberExpression m)
        {
            string _field = string.Format("[{0}]", m.Member.Name);
            if (this.AttributeConvertHandler != null)
            {
                _field = this.AttributeConvertHandler(m.Member);
            }

            if (this.UseTableAlias)
                return string.Format("{0}.[{1}]", m.Expression.ToString(), m.Member.Name);
            else
                return _field;
        }
        private string FindPrefix(string p)
        {
            if (!this._TablePrefix.ContainsKey(p))
            {
                this._TableCount++;
                this._TablePrefix.Add(p, "T" + this._TableCount);
            }
            return this._TablePrefix[p];
        }
        private string GetParameterName(string prefix, string fieldName)
        {
            string _prefix = FindPrefix(prefix);
            int _index = 0;
            string _prename = string.Format("@{0}_{1}_", _prefix, fieldName);
            string _name = this._parameters.Keys.Where(p => p.Contains(_prename)).LastOrDefault();
            if (!string.IsNullOrEmpty(_name))
            {
                string _num = _name.Split('_').LastOrDefault();
                _index = Convert.ToInt32(_num) + 1;
            }
            _name = string.Format(_prename + "{0}", _index);
            return _name;
        }
        protected bool IsNullConstant(Expression exp)
        {
            return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
        }
        private bool IsEmptyValue(object p)
        {
            if (!p.GetType().IsValueType)
            {
                if (p.GetType() == typeof(string))
                {
                    return string.IsNullOrEmpty(p.ToString());
                }
                else if (p.GetType() == typeof(DateTime))
                {
                    return false;
                }

            }
            return p.Equals(null);

        }
        private object GetValue(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();

            return getter();
        }
        private enum NodeDirect
        {
            Right,
            Left
        }


       public Dictionary<string, object> Parameters
        {
            get
            {
                return this._parameters;
            }
            set
            {
                this._parameters = value;
            }
        }

    }

    public class QueryCondition
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public string Parameter { get; set; }
    }

    public class QueryOptCondition : QueryCondition
    {
        public new string Field
        {
            get { return string.Empty; }
        }
        public new string Parameter
        {
            get { return string.Empty; }
        }
    }
}
