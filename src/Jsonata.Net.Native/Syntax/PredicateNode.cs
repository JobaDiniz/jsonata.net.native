using System;
using System.Collections.Generic;
using System.Linq;

namespace Jsonata.Net.Native.Syntax;

// A predicateNode is an interim data structure used when processing
// predicate expressions. It is deliberately unexported and gets
// converted into a PredicateNode during optimization.
internal sealed class PredicateNode_ : Node
{
    private readonly Node lhs;
    private readonly Node rhs;

    // lhs Node - the context for this predicate
    // rhs Node -  the predicate expression
    internal PredicateNode_(Node lhs, Node rhs)
    {
        this.lhs = lhs;
        this.rhs = rhs;
    }


    internal override Node optimize()
    {
        Node lhs = this.lhs.optimize();
        Node rhs = this.rhs.optimize();

        switch (lhs)
        {
            case GroupNode:
                throw new JsonataException("S0209", "A predicate cannot follow a grouping expression in a step"); //TODO: check if this is really not allowed
            case PathNode pathNode:
                {
                    Node last = pathNode.Steps[pathNode.Steps.Count - 1];
                    switch (last)
                    {
                        case PredicateNode predicateLastNode:
                            predicateLastNode.filters.Add(rhs);
                            break;
                        default:
                            pathNode.ReplaceLastStep(new PredicateNode(expr: last, filters: new List<Node>() { rhs }));
                            break;
                    }
                }
                return pathNode;
            default:
                return new PredicateNode(expr: lhs, filters: new List<Node>() { rhs });
        }
    }

    public override string ToString()
    {
        return $"{this.lhs}[{this.rhs}]";
    }

    public override bool Equals(Node? other)
    {
        return other is PredicateNode_ otherPred &&
               this.lhs.Equals(otherPred.lhs) &&
               this.rhs.Equals(otherPred.rhs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(lhs, rhs);
    }
}

// A PredicateNode represents a predicate expression.
internal sealed class PredicateNode : Node
{
    public Node expr { get; }
    public List<Node> filters { get; }

    public PredicateNode(Node expr, List<Node> filters)
    {
        this.expr = expr;
        this.filters = filters;
    }

    internal override Node optimize()
    {
        return this;
    }

    public override string ToString()
    {
        return $"{this.expr}[{this.filters.JoinNodes(", ")}]";
    }

    public override bool Equals(Node? other)
    {
        return other is PredicateNode otherPred &&
               this.expr.Equals(otherPred.expr) &&
               NodeListExtensions.NodeListsEqual(this.filters, otherPred.filters);
    }

    public override int GetHashCode()
    {
        var filtersHash = filters.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode()));
        return HashCode.Combine(expr, filtersHash);
    }
}
