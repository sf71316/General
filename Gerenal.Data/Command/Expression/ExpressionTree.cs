using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    public class ExpressionTree
    {
        private ExpressionNode _rootNode = null;

        public ExpressionTree()
        {
        }

        public ExpressionTree(ExpressionNode RootNode)
        {
            this._rootNode = RootNode;
            this._rootNode.UpdateNodeLevel(0);
        }

        public Queue<ExpressionNode> Traversal(ExpressionTraversalType TraversalType)
        {
            switch (TraversalType)
            {
                case ExpressionTraversalType.LMR:
                    return this.DoLeftMediumRightTraversal();
                case ExpressionTraversalType.MLR:
                    return this.DoMediumLeftRightTraversal();
                case ExpressionTraversalType.LRM:
                    return this.DoLeftRightMediumTraversal();
            }

            return null;
        }

        // front order.
        private Queue<ExpressionNode> DoLeftMediumRightTraversal()
        {
            if (this._rootNode == null)
                return null;

            Queue<ExpressionNode> queue = new Queue<ExpressionNode>();

            this.DoLeftMediumRightTraversal(ref queue, this._rootNode);

            return queue;
        }

        private void DoLeftMediumRightTraversal(ref Queue<ExpressionNode> Queue, ExpressionNode Node)
        {
            // traversal left
            if (Node.ExpressionLeft != null)
                this.DoLeftMediumRightTraversal(ref Queue, Node.ExpressionLeft);

            // add medium node.
            Queue.Enqueue(Node);

            // traversal right.
            if (Node.ExpressionRight != null)
                this.DoLeftMediumRightTraversal(ref Queue, Node.ExpressionRight);
        }

        // medium order.
        private Queue<ExpressionNode> DoMediumLeftRightTraversal()
        {
            if (this._rootNode == null)
                return null;

            Queue<ExpressionNode> queue = new Queue<ExpressionNode>();

            this.DoMediumLeftRightTraversal(ref queue, this._rootNode);

            return queue;
        }

        private void DoMediumLeftRightTraversal(ref Queue<ExpressionNode> Queue, ExpressionNode Node)
        {
            // add medium node.
            Queue.Enqueue(Node);

            // traversal left
            if (Node.ExpressionLeft != null)
                this.DoMediumLeftRightTraversal(ref Queue, Node.ExpressionLeft);

            // traversal right.
            if (Node.ExpressionRight != null)
                this.DoMediumLeftRightTraversal(ref Queue, Node.ExpressionRight);
        }

        // back order.
        private Queue<ExpressionNode> DoLeftRightMediumTraversal()
        {
            if (this._rootNode == null)
                return null;

            Queue<ExpressionNode> queue = new Queue<ExpressionNode>();

            this.DoLeftRightMediumTraversal(ref queue, this._rootNode);

            return queue;
        }

        private void DoLeftRightMediumTraversal(ref Queue<ExpressionNode> Queue, ExpressionNode Node)
        {
            // traversal left
            if (Node.ExpressionLeft != null)
                this.DoLeftRightMediumTraversal(ref Queue, Node.ExpressionLeft);

            // traversal right.
            if (Node.ExpressionRight != null)
                this.DoLeftRightMediumTraversal(ref Queue, Node.ExpressionRight);

            // add medium node.
            Queue.Enqueue(Node);
        }

        public string BuildExpressionEval(ExpressionTraversalType TraversalType)
        {
            switch (TraversalType)
            {
                case ExpressionTraversalType.LMR:
                    return this.BuildExpressionEvalLMR(this._rootNode);
                case ExpressionTraversalType.MLR:
                    return this.BuildExpressionEvalMLR(this._rootNode);
                case ExpressionTraversalType.LRM:
                    return this.BuildExpressionEvalLRM(this._rootNode);
            }

            return null;
        }

        private string BuildExpressionEvalLMR(ExpressionNode Node)
        {
            if (Node.NodeType == ExpressionNodeType.Parameter)
                return (Node as ParameterExpressionNode).Value;
            if (Node.NodeType == ExpressionNodeType.Value)
            {
                DataExpressionNode dataNode = Node as DataExpressionNode;

                if (dataNode.DataType == typeof(string))
                    return string.Format("'{0}'", (Node as DataExpressionNode).Value.ToString());
                else
                    return (Node as DataExpressionNode).Value.ToString();
            }

            StringBuilder evalBuilder = new StringBuilder();

            evalBuilder.Append("(");
            evalBuilder.Append(this.BuildExpressionEvalLMR(Node.ExpressionLeft));
            evalBuilder.Append(this.EvalOperator(Node.NodeType));
            evalBuilder.Append(this.BuildExpressionEvalLMR(Node.ExpressionRight));
            evalBuilder.Append(")");

            return evalBuilder.ToString();
        }

        private string BuildExpressionEvalMLR(ExpressionNode Node)
        {
            if (Node.NodeType == ExpressionNodeType.Parameter)
                return (Node as ParameterExpressionNode).Value;
            if (Node.NodeType == ExpressionNodeType.Value)
            {
                DataExpressionNode dataNode = Node as DataExpressionNode;

                if (dataNode.DataType == typeof(string))
                    return string.Format("'{0}'", (Node as DataExpressionNode).Value.ToString());
                else
                    return (Node as DataExpressionNode).Value.ToString();
            }

            StringBuilder evalBuilder = new StringBuilder();

            evalBuilder.Append(this.EvalOperator(Node.NodeType));
            evalBuilder.Append(this.BuildExpressionEvalMLR(Node.ExpressionLeft));
            evalBuilder.Append(this.BuildExpressionEvalMLR(Node.ExpressionRight));

            return evalBuilder.ToString();
        }

        private string BuildExpressionEvalLRM(ExpressionNode Node)
        {
            if (Node.NodeType == ExpressionNodeType.Parameter)
                return (Node as ParameterExpressionNode).Value;
            if (Node.NodeType == ExpressionNodeType.Value)
            {
                DataExpressionNode dataNode = Node as DataExpressionNode;

                if (dataNode.DataType == typeof(string))
                    return string.Format("'{0}'", (Node as DataExpressionNode).Value.ToString());
                else
                    return (Node as DataExpressionNode).Value.ToString();
            }

            StringBuilder evalBuilder = new StringBuilder();

            evalBuilder.Append(this.BuildExpressionEvalLRM(Node.ExpressionLeft));
            evalBuilder.Append(this.BuildExpressionEvalLRM(Node.ExpressionRight));
            evalBuilder.Append(this.EvalOperator(Node.NodeType));

            return evalBuilder.ToString();
        }

        private string EvalOperator(ExpressionNodeType NodeType)
        {
            switch (NodeType)
            {
                case ExpressionNodeType.Add:
                    return (" + ");
                case ExpressionNodeType.Division:
                    return (" / ");
                case ExpressionNodeType.LogicalAnd:
                    return (" AND ");
                case ExpressionNodeType.LogicalNot:
                    return (" NOT ");
                case ExpressionNodeType.LogicalOr:
                    return (" OR ");
                case ExpressionNodeType.Multiply:
                    return (" * ");
                case ExpressionNodeType.Equal:
                    return (" = ");
                case ExpressionNodeType.NotEqual:
                    return (" <> ");
                case ExpressionNodeType.LessThan:
                    return (" < ");
                case ExpressionNodeType.LessThanEqual:
                    return (" <= ");
                case ExpressionNodeType.GreaterThan:
                    return (" > ");
                case ExpressionNodeType.GreaterThanEqual:
                    return (" >= ");
                case ExpressionNodeType.Subtract:
                    return (" - ");
                default:
                    return " ";
            }
        }
    }
}
