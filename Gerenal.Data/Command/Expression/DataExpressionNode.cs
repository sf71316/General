using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    public class DataExpressionNode : ExpressionNode
    {
        public Type DataType { get; private set; }
        public object Value { get; set; }

        public DataExpressionNode(object Value)
        {
            this.NodeType = ExpressionNodeType.Value;
            this.Value = Value;
            this.DataType = Value.GetType();
        }

        public DataExpressionNode(Type ValueType, object Value)
        {
            this.NodeType = ExpressionNodeType.Value;
            this.Value = Value;
            this.DataType = ValueType;
        }

        public DataExpressionNode(object Value, ExpressionNode ExpressionLeft, ExpressionNode ExpressionRight)
            : this(Value)
        {
            this.ExpressionLeft = ExpressionLeft;
            this.ExpressionRight = ExpressionRight;
        }

        public DataExpressionNode(Type ValueType, object Value, ExpressionNode ExpressionLeft, ExpressionNode ExpressionRight)
            : this(ValueType, Value)
        {
            this.ExpressionLeft = ExpressionLeft;
            this.ExpressionRight = ExpressionRight;
        }
    }
}
