using System;

namespace Jsonata.Net.Native.Syntax;

// A FunctionApplicationNode represents a function application
// operation.
internal sealed class FunctionApplicationNode : Node
{
    public Node lhs { get; }
    public Node rhs { get; }

    public FunctionApplicationNode(Node lhs, Node rhs)
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
            return new FunctionApplicationNode(lhs, rhs);
        }
        else
        {
            return this;
        }
    }

    public override string ToString()
    {
        return $"{this.lhs} ~> {this.rhs}";
    }

    public override bool Equals(Node? other)
    {
        return other is FunctionApplicationNode otherApp &&
               this.lhs.Equals(otherApp.lhs) &&
               this.rhs.Equals(otherApp.rhs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(lhs, rhs);
    }
}
