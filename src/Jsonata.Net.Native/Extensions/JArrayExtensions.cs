using System.Collections.Generic;
using Jsonata.Net.Native.Json;

namespace Jsonata.Net.Native.Extensions;

/// <summary>
/// Extension methods for JArray.
/// </summary>
internal static class JArrayExtensions
{
    /// <summary>
    /// Adds a range of JToken values to the array.
    /// </summary>
    /// <param name="array">The array to add to</param>
    /// <param name="values">The values to add</param>
    public static void AddRange(this JArray array, IEnumerable<JToken> values)
    {
        foreach (JToken value in values)
        {
            array.Add(value);
        }
    }
}