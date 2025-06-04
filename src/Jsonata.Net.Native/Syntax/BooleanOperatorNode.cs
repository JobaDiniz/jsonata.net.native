using System;

namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents a boolean logic expression in a JSONata query.
/// Performs AND and OR operations on boolean-compatible operands.
/// </summary>
internal sealed class BooleanOperatorNode : Node
{
    public Operator op { get; }
    public Node lhs { get; }
    public Node rhs { get; }

    internal enum Operator
    {
        And,
        Or,
    }

    public static string OperatorToString(Operator op) => op switch
    {
        Operator.And => "and",
        Operator.Or => "or",
        _ => throw new ArgumentException($"Unexpected operator '{op}'")
    };

    public BooleanOperatorNode(Operator op, Node lhs, Node rhs)
    {
        this.op = op;
        this.lhs = lhs;
        this.rhs = rhs;
    }

    internal override Node optimize()
    {
        Node lhs = this.lhs.optimize();
        Node rhs = this.rhs.optimize();

        if (lhs != this.lhs || rhs != this.rhs)
        {
            return new BooleanOperatorNode(this.op, lhs, rhs);
        }
        else
        {
            return this;
        }
    }

    public override string ToString()
    {
        return $"{this.lhs} {OperatorToString(this.op)} {this.rhs}";
    }

    public override bool Equals(Node? other)
    {
        return other is BooleanOperatorNode otherOp &&
               this.op == otherOp.op &&
               this.lhs.Equals(otherOp.lhs) &&
               this.rhs.Equals(otherOp.rhs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(op, lhs, rhs);
    }
}
