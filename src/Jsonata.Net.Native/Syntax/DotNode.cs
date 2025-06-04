using System;
using System.Collections.Generic;

namespace Jsonata.Net.Native.Syntax;

// A dotNode is an interim structure used to process JSONata path
// expressions. It is deliberately unexported and creates a PathNode
// during its optimize phase.
internal sealed class DotNode_ : Node
{
    private readonly Node lhs;
    private readonly Node rhs;

    internal DotNode_(Node lhs, Node rhs)
    {
        this.lhs = lhs;
        this.rhs = rhs;
    }

    internal override Node optimize()
    {
        List<Node> steps = new List<Node>();
        bool keepArrays = false;

        //lhs
        {
            Node lhs = this.lhs.optimize();
            switch (lhs)
            {
                case NumberDoubleNode:
                case NumberIntNode:
                case BooleanNode:
                case NullNode:
                    throw new JsonataException("S0213", $"The literal value {lhs} cannot be used as a step within a path expression");
                case StringNode stringNode:
                    //convert string to NameNode https://github.com/IBM/JSONata4Java/issues/25
                    steps.Add(new FieldNameNode(stringNode.value, escaped: true));
                    break;
                case PathNode pathNode:
                    steps.AddRange(pathNode.steps);
                    keepArrays |= pathNode.keepArrays;
                    break;
                default:
                    steps.Add(lhs);
                    break;
            }
        }

        //rhs
        {
            Node rhs = this.rhs.optimize();
            switch (rhs)
            {
                case NumberDoubleNode:
                case NumberIntNode:
                case BooleanNode:
                case NullNode:
                    throw new JsonataException("S0213", $"The literal value {rhs} cannot be used as a step within a path expression");
                case StringNode stringNode:
                    //convert string to NameNode https://github.com/IBM/JSONata4Java/issues/25
                    steps.Add(new FieldNameNode(stringNode.value, escaped: true));
                    break;
                case PathNode pathNode:
                    steps.AddRange(pathNode.steps);
                    keepArrays |= pathNode.keepArrays;
                    break;
                default:
                    steps.Add(rhs);
                    break;
            }
        }

        return new PathNode(steps, keepArrays);
    }

    public override string ToString()
    {
        return $"{this.lhs}.{this.rhs}";
    }

    public override bool Equals(Node? other)
    {
        return other is DotNode_ otherDot &&
               this.lhs.Equals(otherDot.lhs) &&
               this.rhs.Equals(otherDot.rhs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(lhs, rhs);
    }
}
