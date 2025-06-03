using System;
using System.Collections.Generic;
using System.Linq;
using Jsonata.Net.Native.Dom;

namespace Jsonata.Net.Native.Extensions;

/// <summary>
/// Extension methods for Node collections.
/// </summary>
internal static class NodeListExtensions
{
    /// <summary>
    /// Joins nodes into a string using the specified separator.
    /// </summary>
    /// <param name="nodes">The collection of nodes to join</param>
    /// <param name="separator">The separator string</param>
    /// <returns>Joined string representation</returns>
    internal static string JoinNodes(this IReadOnlyList<Node> nodes, string separator)
    {
        return String.Join(separator, nodes.Select(n => n.ToString()));
    }

    /// <summary>
    /// Compares two node lists for equality.
    /// </summary>
    /// <param name="a">First node list</param>
    /// <param name="b">Second node list</param>
    /// <returns>True if the lists are equal</returns>
    internal static bool NodeListsEqual(IReadOnlyList<Node> a, IReadOnlyList<Node> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }
        for (int i = 0; i < a.Count; ++i)
        {
            if (!a[i].Equals(b[i]))
            {
                return false;
            }
        }
        return true;
    }
}