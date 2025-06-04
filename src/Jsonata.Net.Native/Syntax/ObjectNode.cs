using System;
using System.Collections.Generic;
using System.Linq;

namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents an object constructor expression in a JSONata query.
/// Contains key-value pairs where both keys and values are expression nodes.
/// </summary>
internal sealed class ObjectNode : Node
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

    public override bool Equals(Node? other)
    {
        if (other is not ObjectNode otherObj || this.pairs.Count != otherObj.pairs.Count)
        {
            return false;
        }

        //TODO: in case order is not preserved, this will not work
        for (int i = 0; i < this.pairs.Count; ++i)
        {
            if (!this.pairs[i].Item1.Equals(otherObj.pairs[i].Item1) ||
                !this.pairs[i].Item2.Equals(otherObj.pairs[i].Item2))
            {
                return false;
            }
        }
        return true;
    }

    public override int GetHashCode()
    {
        return pairs.Aggregate(0, (hash, pair) => 
            HashCode.Combine(hash, pair.Item1.GetHashCode(), pair.Item2.GetHashCode()));
    }
}
