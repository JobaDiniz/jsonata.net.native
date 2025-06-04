using System;
using System.Collections.Generic;
using System.Linq;

namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents a function call expression in a JSONata query.
/// Contains the function expression and the argument list to be evaluated.
/// </summary>
internal sealed class FunctionCallNode : Node
{
    public Node func { get; }
    public IReadOnlyList<Node> args { get; }

    public FunctionCallNode(Node func, IReadOnlyList<Node> args)
    {
        this.func = func;
        this.args = args;
    }

    //shorthand constructor for manual DOM construction
    public FunctionCallNode(string functionName, IReadOnlyList<Node> args)
        : this(new VariableNode(functionName), args)
    {
    }

    internal override Node optimize()
    {
        Node func = this.func.optimize();
        List<Node> args = this.args.Select(a => a.optimize()).ToList();
        return new FunctionCallNode(func, args);
    }

    public override string ToString()
    {
        return $"{this.func}({this.args.JoinNodes(", ")})";
    }

    public override bool Equals(Node? other)
    {
        return other is FunctionCallNode otherCall &&
               this.func.Equals(otherCall.func) &&
               NodeListExtensions.NodeListsEqual(this.args, otherCall.args);
    }

    public override int GetHashCode()
    {
        var argsHash = args.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode()));
        return HashCode.Combine(func, argsHash);
    }
}
