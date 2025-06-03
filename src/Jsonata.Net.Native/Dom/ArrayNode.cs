using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jsonata.Net.Native.Parsing;

namespace Jsonata.Net.Native.Dom;

// An ArrayNode represents an array of items.
public sealed class ArrayNode : Node
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
        return "[" + Helpers.JoinNodes(this.Items, ", ") + "]";
    }

    protected override bool EqualsSpecific(Node other)
    {
        ArrayNode otherNode = (ArrayNode)other;
        return Helpers.NodeListsEqual(this.items, otherNode.items);
    }
}
