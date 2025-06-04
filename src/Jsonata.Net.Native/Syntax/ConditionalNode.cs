using System;

namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents a conditional (ternary) expression in a JSONata query.
/// Evaluates a predicate and returns one of two alternative expressions based on the result.
/// </summary>
internal sealed class ConditionalNode : Node
{
    public Node predicate { get; }
    public Node thenExpr { get; }
    public Node? elseExpr { get; }

    public ConditionalNode(Node predicate, Node thenExpr, Node? elseExpr)
    {
        this.predicate = predicate;
        this.thenExpr = thenExpr;
        this.elseExpr = elseExpr;
    }

    internal override Node optimize()
    {
        Node predicate = this.predicate.optimize();
        Node expr1 = this.thenExpr.optimize();
        Node? expr2 = this.elseExpr?.optimize();

        if (predicate != this.predicate
            || expr1 != this.thenExpr
            || expr2 != this.elseExpr
        )
        {
            return new ConditionalNode(predicate, expr1, expr2);
        }
        else
        {
            return this;
        }
    }

    public override string ToString()
    {
        if (this.elseExpr != null)
        {
            return $"{this.predicate} ? {this.thenExpr} : {this.elseExpr}";
        }
        else
        {
            return $"{this.predicate} ? {this.thenExpr}";
        }
    }

    public override bool Equals(Node? other)
    {
        return other is ConditionalNode otherCond &&
               this.predicate.Equals(otherCond.predicate) &&
               this.thenExpr.Equals(otherCond.thenExpr) &&
               ((this.elseExpr == null && otherCond.elseExpr == null) ||
                (this.elseExpr != null && otherCond.elseExpr != null && this.elseExpr.Equals(otherCond.elseExpr)));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(predicate, thenExpr, elseExpr);
    }
}
