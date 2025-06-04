using System;
using System.Collections.Generic;
using System.Linq;

namespace Jsonata.Net.Native.Syntax;

// A PartialNode represents a partially applied function.
internal sealed class PartialApplicationNode : Node
{
    public Node func { get; }
    public IReadOnlyList<Node> args { get; }

    public PartialApplicationNode(Node func, IReadOnlyList<Node> args)
    {
        this.func = func;
        this.args = args;
    }

    internal override Node optimize()
    {
        Node func = this.func.optimize();
        List<Node> args = this.args.Select(a => a.optimize()).ToList();
        return new PartialApplicationNode(func, args);
    }

    public override string ToString()
    {
        return $"{this.func}({this.args.JoinNodes(", ")})";
    }

    public override bool Equals(Node? other)
    {
        return other is PartialApplicationNode otherPartial &&
               this.func.Equals(otherPartial.func) &&
               NodeListExtensions.NodeListsEqual(this.args, otherPartial.args);
    }

    public override int GetHashCode()
    {
        var argsHash = args.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode()));
        return HashCode.Combine(func, argsHash);
    }
}
