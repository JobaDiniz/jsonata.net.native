using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Jsonata.Net.Native.Json;

namespace Jsonata.Net.Native.Tests;

public class FunctionTokenCsharpShould
{
    [Fact]
    public void Automatically_Detect_Functions_With_EvaluationEnvironment_Parameters()
    {
        // Arrange
        var methodInfo = typeof(TestHelpers).GetMethod(nameof(TestHelpers.CaptureEnvironmentVariable));
        
        // Act
        var functionToken = new FunctionTokenCsharp("testFunc", methodInfo!);
        
        // Assert
        Assert.Equal(1, functionToken.RequiredArgsCount);
    }

    [Fact]
    public void Inject_Current_Environment_Instance_To_Function_Parameters()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindValue("testVar", new JValue("injected_value"));
        var methodInfo = typeof(TestHelpers).GetMethod(nameof(TestHelpers.CaptureEnvironmentVariable));
        var functionToken = new FunctionTokenCsharp("testFunc", methodInfo!);
        var args = new List<JToken> { new JValue("testVar") };
        
        // Act
        var result = functionToken.Invoke(args, null, environment);
        
        // Assert
        Assert.Equal("\"injected_value\"", result.ToFlatString());
    }

    [Fact]
    public void Exclude_EvaluationEnvironment_Parameters_From_Required_Argument_Count()
    {
        // Arrange
        var methodWithEnv = typeof(TestHelpers).GetMethod(nameof(TestHelpers.MixedParameters));
        var methodWithoutEnv = typeof(TestHelpers).GetMethod(nameof(TestHelpers.RegularFunction));
        
        // Act
        var tokenWithEnv = new FunctionTokenCsharp("withEnv", methodWithEnv!);
        var tokenWithoutEnv = new FunctionTokenCsharp("withoutEnv", methodWithoutEnv!);
        
        // Assert
        Assert.Equal(2, tokenWithEnv.RequiredArgsCount);
        Assert.Equal(1, tokenWithoutEnv.RequiredArgsCount);
    }

    [Fact]
    public void Handle_Mixed_Regular_And_EvaluationEnvironment_Parameters()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindValue("contextValue", new JValue("context_data"));
        var methodInfo = typeof(TestHelpers).GetMethod(nameof(TestHelpers.MixedParameters));
        var functionToken = new FunctionTokenCsharp("testFunc", methodInfo!);
        var args = new List<JToken> { new JValue("test"), new JValue(42) };
        
        // Act
        var result = functionToken.Invoke(args, null, environment);
        
        // Assert
        Assert.Equal("\"test_42_context_data\"", result.ToFlatString());
    }

    [Fact]
    public void Only_Inject_Parameters_Of_Exact_EvaluationEnvironment_Type()
    {
        // Arrange
        var methodInfo = typeof(TestHelpers).GetMethod(nameof(TestHelpers.WrongTypeParameter));
        var functionToken = new FunctionTokenCsharp("testFunc", methodInfo!);
        
        // Act & Assert
        Assert.Equal(2, functionToken.RequiredArgsCount);
    }

    [Fact]
    public void Report_Correct_Argument_Count_Errors_With_Automatic_Parameters()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        var methodInfo = typeof(TestHelpers).GetMethod(nameof(TestHelpers.MixedParameters));
        var functionToken = new FunctionTokenCsharp("testFunc", methodInfo!);
        var args = new List<JToken> { new JValue("test") }; // Missing the multiplier argument
        
        // Act & Assert
        var ex = Assert.Throws<JsonataException>(() => functionToken.Invoke(args, null, environment));
        Assert.Contains("requires 2 arguments", ex.Message);
    }
}