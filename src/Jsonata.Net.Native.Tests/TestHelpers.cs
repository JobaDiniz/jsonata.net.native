using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Eval;
using Jsonata.Net.Native.Functions;

namespace Jsonata.Net.Native.Tests;

/// <summary>
/// Helper functions for testing automatic environment injection functionality.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Test function that captures and returns a value from the injected environment.
    /// </summary>
    [FunctionName("captureEnvironmentVar")]
    public static JToken CaptureEnvironmentVariable(string variableName, EvaluationEnvironment environment)
    {
        var result = environment.Lookup(variableName);
        return result.Type == JTokenType.Undefined ? JValue.Undefined : result;
    }

    /// <summary>
    /// Test function with mixed regular and environment parameters.
    /// </summary>
    [FunctionName("mixedParams")]
    public static JToken MixedParameters(string input, int multiplier, EvaluationEnvironment environment)
    {
        var contextVar = environment.Lookup("contextValue");
        var contextValue = contextVar.Type == JTokenType.Undefined ? "default" : (string)contextVar;
        return new JValue($"{input}_{multiplier}_{contextValue}");
    }

    /// <summary>
    /// Test function that resolves another function from the environment.
    /// </summary>
    [FunctionName("resolveFunction")]
    public static JToken ResolveFunctionFromEnvironment(string functionName, EvaluationEnvironment environment)
    {
        var function = environment.Lookup(functionName);
        return function.Type == JTokenType.Undefined ? new JValue("not_found") : new JValue("found");
    }

    /// <summary>
    /// Test function that creates a child environment and tests isolation.
    /// </summary>
    [FunctionName("testIsolation")]
    public static JToken TestEnvironmentIsolation(string newVarName, string newVarValue, EvaluationEnvironment environment)
    {
        var childEnv = environment.CreateChildForBlock();
        childEnv.BindValue(newVarName, new JValue(newVarValue));
        
        // Check if new variable exists in child
        var inChildToken = childEnv.Lookup(newVarName);
        var inChild = inChildToken.Type == JTokenType.Undefined ? "undefined" : (string)inChildToken;
        
        // Check if new variable exists in parent (should not)
        var inParentToken = environment.Lookup(newVarName);
        var inParent = inParentToken.Type == JTokenType.Undefined ? "undefined" : (string)inParentToken;
        
        return new JValue($"child:{inChild},parent:{inParent}");
    }

    /// <summary>
    /// Regular function without environment parameter for comparison.
    /// </summary>
    [FunctionName("regularFunction")]
    public static JToken RegularFunction(string input)
    {
        return new JValue($"regular_{input}");
    }

    /// <summary>
    /// Function that simulates wrong parameter type (not EvaluationEnvironment).
    /// </summary>
    [FunctionName("wrongType")]
    public static JToken WrongTypeParameter(string input, string notEnvironment)
    {
        return new JValue($"{input}_{notEnvironment}");
    }
}