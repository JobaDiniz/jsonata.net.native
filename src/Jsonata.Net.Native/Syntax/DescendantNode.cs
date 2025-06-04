namespace Jsonata.Net.Native.Syntax;

// A DescendentNode represents the descendant operator.
internal sealed class DescendantNode : Node
{
    public DescendantNode() { }

    internal override Node optimize()
    {
        return this;
    }

    public override string ToString()
    {
        return "**";
    }

    public override bool Equals(Node? other)
    {
        return other is DescendantNode;
    }

    public override int GetHashCode()
    {
        return typeof(DescendantNode).GetHashCode();
    }
}
