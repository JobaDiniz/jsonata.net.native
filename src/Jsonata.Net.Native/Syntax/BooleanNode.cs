namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents a boolean literal expression in a JSONata query.
/// Contains a true or false value as a constant node.
/// </summary>
internal sealed class BooleanNode : Node
{
    public bool value { get; }

    public BooleanNode(bool value)
    {
        this.value = value;
    }

    internal override Node optimize()
    {
        return this;
    }

    public override string ToString()
    {
        return this.value.ToString();
    }

    public override bool Equals(Node? other)
    {
        return other is BooleanNode otherBool && this.value == otherBool.value;
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
}
