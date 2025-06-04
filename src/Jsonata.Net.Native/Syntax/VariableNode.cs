namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents a variable reference expression in a JSONata query.
/// Contains the variable name to be resolved from the evaluation context.
/// </summary>
internal sealed class VariableNode : Node
{
    public string name { get; }

    public VariableNode(string name)
    {
        this.name = name;
    }

    internal override Node optimize()
    {
        return this;
    }

    public override string ToString()
    {
        return "$" + this.name;
    }

    public override bool Equals(Node? other)
    {
        return other is VariableNode otherVar && this.name == otherVar.name;
    }

    public override int GetHashCode()
    {
        return name.GetHashCode();
    }
}
