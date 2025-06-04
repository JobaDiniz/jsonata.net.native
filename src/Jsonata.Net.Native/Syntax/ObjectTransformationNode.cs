using System;
﻿namespace Jsonata.Net.Native.Syntax;

internal sealed class ObjectTransformationNode : Node
{
    public Node pattern { get; }
    public Node updates { get; }
    public Node? deletes { get; }

    public ObjectTransformationNode(Node pattern, Node updates, Node? deletes)
    {
        this.pattern = pattern;
        this.updates = updates;
        this.deletes = deletes;
    }

    internal override Node optimize()
    {
        Node pattern = this.pattern.optimize();
        Node updates = this.updates.optimize();
        Node? deletes = this.deletes?.optimize();
        if (pattern != this.pattern
            || updates != this.updates
            || deletes != this.deletes)
        {
            return new ObjectTransformationNode(pattern, updates, deletes);
        }
        else
        {
            return this;
        }
    }

    public override string ToString()
    {
        if (this.deletes != null)
        {
            return $"|{this.pattern}|{this.updates}, {this.deletes}|";
        }
        else
        {
            return $"|{this.pattern}|{this.updates}|";
        }
    }

    public override bool Equals(Node? other)
    {
        return other is ObjectTransformationNode otherTransform &&
               this.pattern.Equals(otherTransform.pattern) &&
               this.updates.Equals(otherTransform.updates) &&
               ((this.deletes == null && otherTransform.deletes == null) ||
                (this.deletes != null && otherTransform.deletes != null && this.deletes.Equals(otherTransform.deletes)));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(pattern, updates, deletes);
    }
}
