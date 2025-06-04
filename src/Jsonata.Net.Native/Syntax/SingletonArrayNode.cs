using System;
using System.Collections.Generic;

namespace Jsonata.Net.Native.Syntax;

// A singletonArrayNode is an interim data structure used when
// processing path expressions. It is deliberately unexported
// and gets converted into a PathNode during optimization.
internal sealed class SingletonArrayNode_ : Node
{
    private readonly Node lhs;

    internal SingletonArrayNode_(Node lhs)
    {
        this.lhs = lhs;
    }

    internal override Node optimize()
    {
        Node lhs = this.lhs.optimize();
        switch (lhs)
        {
            case PathNode pathNode:
                if (pathNode.keepArrays)
                {
                    return pathNode;
                }
                return pathNode.CloneWithKeepArrays();
            default:
                return new PathNode(new List<Node>() { lhs }, keepArrays: true);
        }
    }

    public override string ToString()
    {
        return $"{this.lhs}[]";
    }

    public override bool Equals(Node? other)
    {
        return other is SingletonArrayNode_ otherSingleton &&
               this.lhs.Equals(otherSingleton.lhs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(typeof(SingletonArrayNode_), lhs);
    }
}
