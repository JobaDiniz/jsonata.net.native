namespace Jsonata.Net.Native.Syntax;

// A WildcardNode represents the wildcard operator.
internal sealed class WildcardNode() : Node
{
    internal override Node optimize()
    {
        return this;
    }

    public override string ToString()
    {
        return "*";
    }

    public override bool Equals(Node? other)
    {
        return other is WildcardNode;
    }

    public override int GetHashCode()
    {
        return typeof(WildcardNode).GetHashCode();
    }
}
