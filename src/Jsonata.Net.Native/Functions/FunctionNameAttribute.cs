using System;

namespace Jsonata.Net.Native.Functions;

/// <summary>
/// Specifies the JSONata function name for a method when it differs from the C# method name.
/// This allows C# methods to follow .NET naming conventions while preserving JSONata function names.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class FunctionNameAttribute : Attribute
{
    /// <summary>
    /// Gets the JSONata function name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionNameAttribute"/> class.
    /// </summary>
    /// <param name="name">The JSONata function name</param>
    public FunctionNameAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}