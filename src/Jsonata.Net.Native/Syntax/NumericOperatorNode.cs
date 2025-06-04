using System;

namespace Jsonata.Net.Native.Syntax;

internal sealed class NumericOperatorNode : Node
{
    internal enum Operator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo
    }

    public static string OperatorToString(Operator op) => op switch
    {
        Operator.Add => "+",
        Operator.Subtract => "-",
        Operator.Multiply => "*",
        Operator.Divide => "/",
        Operator.Modulo => "%",
        _ => throw new ArgumentException($"Unexpected operator '{op}'")
    };

    public Operator op { get; }
    public Node lhs { get; }
    public Node rhs { get; }


    public NumericOperatorNode(Operator op, Node lhs, Node rhs)
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
            return new NumericOperatorNode(this.op, lhs, rhs);
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
        return other is NumericOperatorNode otherNum &&
               this.op == otherNum.op &&
               this.lhs.Equals(otherNum.lhs) &&
               this.rhs.Equals(otherNum.rhs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(op, lhs, rhs);
    }
}
