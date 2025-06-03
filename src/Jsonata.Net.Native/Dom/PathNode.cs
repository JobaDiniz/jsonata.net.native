using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jsonata.Net.Native.Extensions;

namespace Jsonata.Net.Native.Dom;

// A PathNode represents a JSON object path. It consists of one
// or more 'steps' or Nodes (most commonly NameNode objects).
public sealed class PathNode : Node
{
    internal readonly List<Node> steps;
    public IReadOnlyList<Node> Steps => this.steps;
    public bool keepArrays { get; }

    public PathNode(List<Node> steps, bool keepArrays)
    {
        this.steps = steps;
        this.keepArrays = keepArrays;
    }

    internal override Node optimize()
    {
        return this;
    }

    public override string ToString()
    {
        string result = this.Steps.JoinNodes(".");
        if (this.keepArrays)
        {
            result += "[]";
        }
        return result;
    }

    internal void ReplaceLastStep(PredicateNode replacement)
    {
        this.steps.RemoveAt(this.steps.Count - 1);
        this.steps.Add(replacement);
    }

    internal PathNode CloneWithKeepArrays()
    {
        return new PathNode(this.steps, keepArrays: true);
    }

    protected override bool EqualsSpecific(Node other)
    {
        PathNode otherNode = (PathNode)other;

        return this.keepArrays == otherNode.keepArrays
            && NodeListExtensions.NodeListsEqual(this.steps, otherNode.steps);
    }
}
