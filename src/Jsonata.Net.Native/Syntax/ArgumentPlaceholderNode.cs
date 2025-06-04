namespace Jsonata.Net.Native.Syntax;

// A PlaceholderNode represents a placeholder argument
// in a partially applied function.
internal sealed class ArgumentPlaceholderNode : Node
{
    public ArgumentPlaceholderNode() { }

    internal override Node optimize()
    {
        return this;
    }

    public override string ToString()
    {
        return "?";
    }

    public override bool Equals(Node? other)
    {
        return other is ArgumentPlaceholderNode;
    }

    public override int GetHashCode()
    {
        return typeof(ArgumentPlaceholderNode).GetHashCode();
    }
}
