using System;

namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents an abstract syntax tree node in a JSONata expression.
/// Provides the foundation for all expression nodes in the parsing tree.
/// </summary>
internal abstract class Node : IEquatable<Node>
{
    internal abstract Node optimize();

    // Standard object.Equals override
    public override bool Equals(object? obj) => Equals(obj as Node);

    // Virtual IEquatable<Node> implementation - derived classes override completely
    public virtual bool Equals(Node? other)
    {
        return other?.GetType() == GetType();
    }

    // Each derived class must implement
    public abstract override int GetHashCode();
}
