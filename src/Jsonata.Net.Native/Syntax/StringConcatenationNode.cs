using System;

namespace Jsonata.Net.Native.Syntax;

internal sealed class StringConcatenationNode : Node
{
    public Node lhs { get; }
    public Node rhs { get; }

    public StringConcatenationNode(Node lhs, Node rhs)
    {
        this.lhs = lhs;
        this.rhs = rhs;
    }

    internal override Node optimize()
    {
        Node lhs = this.lhs.optimize();
        Node rhs = this.rhs.optimize();

        if (lhs != this.lhs || rhs != this.rhs)
        {
            return new StringConcatenationNode(lhs, rhs);
        }
        else
        {
            return this;
        }
    }

    public override string ToString()
    {
        return $"{this.lhs} & {this.rhs}";
    }

    public override bool Equals(Node? other)
    {
        return other is StringConcatenationNode otherConcat &&
               this.lhs.Equals(otherConcat.lhs) &&
               this.rhs.Equals(otherConcat.rhs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(lhs, rhs);
    }
}
