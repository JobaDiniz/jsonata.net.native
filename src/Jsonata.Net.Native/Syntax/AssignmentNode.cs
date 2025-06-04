namespace Jsonata.Net.Native.Syntax;

// An AssignmentNode represents a variable assignment.
internal sealed class AssignmentNode : Node
{
    public string name { get; }
    public Node value { get; }

    public AssignmentNode(string name, Node value)
    {
        this.name = name;
        this.value = value;
    }

    internal override Node optimize()
    {
        Node value = this.value.optimize();
        if (value != this.value)
        {
            return new AssignmentNode(this.name, value);
        }
        else
        {
            return this;
        }
    }

    protected override bool EqualsSpecific(Node other)
    {
        AssignmentNode otherNode = (AssignmentNode)other;
        return this.name == otherNode.name
            && this.value.Equals(otherNode.value);
    }
}
