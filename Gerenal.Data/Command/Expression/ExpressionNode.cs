using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    public abstract class ExpressionNode
    {
        public ExpressionNodeType NodeType { get; protected set; }
        public int NodeLevel { get; private set; }
        public ExpressionNode ExpressionLeft { get; protected set; }
        public ExpressionNode ExpressionRight { get; protected set; }

        public bool IsLeafNode
        {
            get { return (this.ExpressionLeft == null && this.ExpressionRight == null); }
        }

        public ExpressionNode()
        {
        }

        public ExpressionNode(string Expression, ExpressionNode ExpressionLeft, ExpressionNode ExpressionRight)
        {
            this.ExpressionLeft = ExpressionLeft;
            this.ExpressionRight = ExpressionRight;
        }

        public void UpdateExpressionLeft(ExpressionNode Node)
        {
            this.ExpressionLeft = Node;
            this.ExpressionLeft.UpdateNodeLevel(this.NodeLevel + 1);
        }

        public void UpdateExpressionRight(ExpressionNode Node)
        {
            this.ExpressionRight = Node;
            this.ExpressionRight.UpdateNodeLevel(this.NodeLevel + 1);
        }

        public void UpdateNodeLevel(int NodeLevel)
        {
            this.NodeLevel = NodeLevel;
        }
    }
}
