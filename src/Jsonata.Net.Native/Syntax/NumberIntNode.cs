namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents an integer literal expression in a JSONata query.
/// Contains a 64-bit signed integer value as a constant node.
/// </summary>
internal sealed class NumberIntNode : NumberNode
{
    public long value { get; }

    public NumberIntNode(long value)
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

    public override int GetIntValue()
    {
        return (int)this.value;
    }

    public override bool Equals(Node? other)
    {
        return other is NumberIntNode otherInt && this.value == otherInt.value;
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
}
