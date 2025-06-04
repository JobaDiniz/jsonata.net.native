using System;

namespace Jsonata.Net.Native.Syntax;

// A RangeNode represents the range operator.
internal sealed class RangeNode : Node
{
    public Node lhs { get; }
    public Node rhs { get; }

    public RangeNode(Node lhs, Node rhs)
    {
        this.lhs = lhs;
        this.rhs = rhs;
    }

    internal override Node optimize()
    {
        Node rhs = this.rhs.optimize();
        Node lhs = this.lhs.optimize();
        if (lhs != this.lhs || rhs != this.rhs)
        {
            return new RangeNode(lhs, rhs);
        }
        else
        {
            return this;
        }
    }

    public override string ToString()
    {
        return this.lhs.ToString() + ".." + this.rhs.ToString();
    }

    public override bool Equals(Node? other)
    {
        return other is RangeNode otherRange &&
               this.lhs.Equals(otherRange.lhs) &&
               this.rhs.Equals(otherRange.rhs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(lhs, rhs);
    }
}
