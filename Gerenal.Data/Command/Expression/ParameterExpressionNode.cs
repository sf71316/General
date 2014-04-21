using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    class ParameterExpressionNode : ExpressionNode
    {
        public string Value { get; set; }

        public ParameterExpressionNode(string Value)
        {
            this.NodeType = ExpressionNodeType.Parameter;
            this.Value = Value;
        }
    }
}
