namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents a string literal expression in a JSONata query.
/// Contains the parsed string value as a constant node.
/// </summary>
internal sealed class StringNode : Node
{
    public string value { get; }

    public StringNode(string value)
    {
        this.value = value;
    }

    internal override Node optimize()
    {
        return this;
    }

    public override string ToString()
    {
        return this.value;
    }

    protected override bool EqualsSpecific(Node other)
    {
        StringNode otherNode = (StringNode)other;
        return otherNode.value == this.value;
    }
}
