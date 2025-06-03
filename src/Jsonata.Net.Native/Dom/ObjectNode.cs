using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jsonata.Net.Native.Dom;

// An ObjectNode represents an object, an unordered list of
// key-value pairs.
public sealed class ObjectNode : Node
{
    internal readonly List<Tuple<Node, Node>> pairs;
    public IReadOnlyList<Tuple<Node, Node>> Pairs => this.pairs;

    public ObjectNode(List<Tuple<Node, Node>> pairs)
    {
        this.pairs = pairs;
    }

    internal override Node optimize()
    {
        for (int i = 0; i < this.pairs.Count; ++i)
        {
            Tuple<Node, Node> pair = this.Pairs[i];
            this.pairs[i] = Tuple.Create(pair.Item1.optimize(), pair.Item2.optimize());
        }
        return this;
    }

    public override string ToString()
    {
        return "{" + String.Join(", ", this.Pairs.Select(p => p.Item1.ToString() + ": " + p.Item2.ToString())) + "}";
    }

    protected override bool EqualsSpecific(Node other)
    {
        ObjectNode otherNode = (ObjectNode)other;

        if (this.pairs.Count != otherNode.pairs.Count)
        {
            return false;
        }

        //TODO: in case order is not preserved, this will not work (
        for (int i = 0; i < this.pairs.Count; ++i)
        {
            if (!this.pairs[i].Item1.Equals(otherNode.pairs[i].Item1)
                || !this.pairs[i].Item2.Equals(otherNode.pairs[i].Item2)
            )
            {
                return false;
            }
        }
        return true;
    }
}
