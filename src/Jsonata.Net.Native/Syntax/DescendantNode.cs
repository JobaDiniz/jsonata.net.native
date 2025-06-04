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

    protected override bool EqualsSpecific(Node other)
    {
        return true;
    }
}
