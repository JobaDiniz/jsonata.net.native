using System;

namespace Jsonata.Net.Native
{
    //provides support for cases when
    // "undefined inputs always return undefined"
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class PropagateUndefinedAttribute : Attribute
    {
    }

    //provides support for cases when
    // "If arg is not specified (i.e. this function is invoked with no arguments), then the context value is used as the value of arg"
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class AllowContextAsValueAttribute : Attribute
    {
    }

    //provides support for cases when
    // function expects an array as an argument, but a non-array is passed as argument value, e.g. in map or filter
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class PackSingleValueToSequenceAttribute : Attribute
    {
    }

    //provides support for cases when
    // "undefined inputs always return undefined"
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class OptionalArgumentAttribute : Attribute
    {
        public readonly object? DefaultValue;

        public OptionalArgumentAttribute(object? defaultValue)
        {
            this.DefaultValue = defaultValue;
        }
    }

    //provides support for builtin functions that require QueryExecutionState
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class ExecutionStateArgumentAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class VariableNumberArgumentAsArrayAttribute : Attribute
    {
    }
}
