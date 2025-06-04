using System;
﻿namespace Jsonata.Net.Native.Syntax;

// A GroupNode represents a group expression.
internal sealed class GroupNode : Node
{
    public Node expr { get; }
    public ObjectNode objectNode { get; }

    public GroupNode(Node expr, ObjectNode objectNode)
    {
        this.expr = expr;
        this.objectNode = objectNode;
    }

    internal override Node optimize()
    {
        Node expr = this.expr.optimize();
        if (expr is GroupNode)
        {
            throw new JsonataException("S0210", "Each step can only have one grouping expression");
        }
        ;

        ObjectNode objectNode = (ObjectNode)this.objectNode.optimize();
        if (this.expr != expr || this.objectNode != objectNode)
        {
            return new GroupNode(expr, objectNode);
        }
        else
        {
            return this;
        }
    }

    public override string ToString()
    {
        return $"{this.expr}{this.objectNode}";
    }

    public override bool Equals(Node? other)
    {
        return other is GroupNode otherGroup &&
               this.expr.Equals(otherGroup.expr) &&
               this.objectNode.Equals(otherGroup.objectNode);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(expr, objectNode);
    }
}
