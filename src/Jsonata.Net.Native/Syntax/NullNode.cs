namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents a null literal expression in a JSONata query.
/// Contains the JSON null value as a constant node.
/// </summary>
internal sealed class NullNode : Node
{
    public NullNode() { }

    internal override Node optimize()
    {
        return this;
    }

    public override string ToString()
    {
        return "null";
    }

    public override bool Equals(Node? other)
    {
        return other is NullNode;
    }

    public override int GetHashCode()
    {
        return typeof(NullNode).GetHashCode();
    }
}
