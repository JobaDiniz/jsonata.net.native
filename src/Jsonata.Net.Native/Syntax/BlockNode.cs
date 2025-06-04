using System;
using System.Collections.Generic;

namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents a block expression in a JSONata query.
/// Contains a sequence of expressions that are evaluated in order, returning the last result.
/// </summary>
internal sealed class BlockNode : Node
{
    internal readonly List<Node> expressions;
    public IReadOnlyList<Node> Expressions => this.expressions;

    public BlockNode(List<Node> expressions)
    {
        this.expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
    }

    internal override Node optimize()
    {
        for (int i = 0; i < this.expressions.Count; ++i)
        {
            this.expressions[i] = this.expressions[i].optimize();
        }
        return this;
    }

    public override string ToString()
    {
        return "(" + this.expressions.JoinNodes("; ") + ")";
    }

    protected override bool EqualsSpecific(Node other)
    {
        BlockNode otherNode = (BlockNode)other;
        return NodeListExtensions.NodeListsEqual(this.expressions, otherNode.expressions);
    }
}
