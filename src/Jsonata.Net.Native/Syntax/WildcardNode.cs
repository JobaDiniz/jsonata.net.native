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

    protected override bool EqualsSpecific(Node other)
    {
        return true;
    }
}
