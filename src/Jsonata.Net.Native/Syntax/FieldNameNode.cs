using System;
using System.Collections.Generic;

namespace Jsonata.Net.Native.Syntax;

// A NameNode represents a JSON field name.
internal sealed class FieldNameNode : Node
{
    public string value { get; }
    public bool escaped { get; }

    public FieldNameNode(string value, bool escaped)
    {
        this.value = value;
        this.escaped = escaped;
    }

    public FieldNameNode(string value)
        : this(value, true)
    {
    }

    internal override Node optimize()
    {
        return new PathNode(new List<Node>() { this }, keepArrays: false);
    }

    public override string ToString()
    {
        return this.escaped ?
            "`" + this.value + "`"
            : this.value;
    }

    public override bool Equals(Node? other)
    {
        return other is FieldNameNode otherField &&
               this.value == otherField.value &&
               this.escaped == otherField.escaped;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(value, escaped);
    }
}
