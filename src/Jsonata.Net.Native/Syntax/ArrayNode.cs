using System;
using System.Collections.Generic;

namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents an array constructor expression in a JSONata query.
/// Contains a sequence of expression nodes that evaluate to array elements.
/// </summary>
internal sealed class ArrayNode : Node
{
    internal readonly List<Node> items;
    public IReadOnlyList<Node> Items => this.items;

    public ArrayNode(List<Node> items)
    {
        this.items = items ?? throw new ArgumentNullException(nameof(items));
    }

    internal override Node optimize()
    {
        for (int i = 0; i < this.items.Count; ++i)
        {
            this.items[i] = this.items[i].optimize();
        }
        return this;
    }

    public override string ToString()
    {
        return "[" + this.Items.JoinNodes(", ") + "]";
    }

    protected override bool EqualsSpecific(Node other)
    {
        ArrayNode otherNode = (ArrayNode)other;
        return NodeListExtensions.NodeListsEqual(this.items, otherNode.items);
    }
}
