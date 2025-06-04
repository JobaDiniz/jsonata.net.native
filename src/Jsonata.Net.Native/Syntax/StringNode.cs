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

    public override bool Equals(Node? other)
    {
        return other is StringNode otherString && this.value == otherString.value;
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
}
