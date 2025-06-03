using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jsonata.Net.Native.Parsing;

namespace Jsonata.Net.Native.Dom;

// A BlockNode represents a block expression.
public sealed class BlockNode : Node
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
        return "(" + Helpers.JoinNodes(this.expressions, "; ") + ")";
    }

    protected override bool EqualsSpecific(Node other)
    {
        BlockNode otherNode = (BlockNode)other;
        return Helpers.NodeListsEqual(this.expressions, otherNode.expressions);
    }
}
