using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    public enum ExpressionNodeType
    {
        Add,
        Subtract,
        Multiply,
        Division,
        Value,
        Parameter,
        Equal,
        NotEqual,
        LessThanEqual,
        GreaterThanEqual,
        LessThan,
        GreaterThan,
        LogicalAnd,
        LogicalOr,
        LogicalNot
    }
}
