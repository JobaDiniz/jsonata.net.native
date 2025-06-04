using System;
using System.Collections.Generic;
using System.Linq;

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

    public override bool Equals(Node? other)
    {
        return other is ArrayNode otherArray && 
               NodeListExtensions.NodeListsEqual(this.items, otherArray.items);
    }

    public override int GetHashCode()
    {
        return items.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode()));
    }
}
