using System;

namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents an abstract syntax tree node in a JSONata expression.
/// Provides the foundation for all expression nodes in the parsing tree.
/// </summary>
internal abstract class Node : IEquatable<Node>
{
    internal abstract Node optimize();

    //used for DOM comparison only
    public bool Equals(Node? other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.GetType() != this.GetType())
        {
            return false;
        }

        return this.EqualsSpecific(other);
    }

    protected abstract bool EqualsSpecific(Node other);
}
