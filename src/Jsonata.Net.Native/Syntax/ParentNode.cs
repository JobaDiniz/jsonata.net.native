using System.Collections.Generic;

namespace Jsonata.Net.Native.Syntax;

// A ParentNode represents a parent loockback.
internal sealed class ParentNode : Node
{
    public ParentNode() { }

    internal override Node optimize()
    {
        return new PathNode(new List<Node>() { this }, keepArrays: false);
    }

    public override string ToString()
    {
        return "%";
    }

    protected override bool EqualsSpecific(Node other)
    {
        return true;
    }
}
