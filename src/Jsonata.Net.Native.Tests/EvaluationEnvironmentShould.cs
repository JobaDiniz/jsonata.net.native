using System;
using System.Reflection;
using Xunit;
using Jsonata.Net.Native.Json;

namespace Jsonata.Net.Native.Tests;

public class EvaluationEnvironmentShould
{
    [Fact]
    public void Resolve_Variables_Following_Parent_Child_Chain()
    {
        // Arrange
        var parentEnv = EvaluationEnvironment.CreateStandard();
        parentEnv.BindValue("parentVar", new JValue("parent_value"));
        var childEnv = parentEnv.CreateChildForBlock();
        childEnv.BindValue("childVar", new JValue("child_value"));
        
        // Act
        var parentVarFromChild = childEnv.Lookup("parentVar");
        var childVarFromChild = childEnv.Lookup("childVar");
        var childVarFromParent = parentEnv.Lookup("childVar");
        
        // Assert
        Assert.Equal("\"parent_value\"", parentVarFromChild.ToFlatString());
        Assert.Equal("\"child_value\"", childVarFromChild.ToFlatString());
        Assert.Equal(JTokenType.Undefined, childVarFromParent.Type);
    }

    [Fact]
    public void Isolate_Child_Environment_Modifications_From_Parent()
    {
        // Arrange
        var parentEnv = EvaluationEnvironment.CreateStandard();
        parentEnv.BindValue("sharedVar", new JValue("original"));
        parentEnv.BindFunction("testIsolation", typeof(TestHelpers).GetMethod(nameof(TestHelpers.TestEnvironmentIsolation))!);
        var query = new JsonataQuery("$testIsolation('newVar', 'new_value')", parentEnv);
        
        // Act
        var result = query.Eval(JValue.CreateNull());
        
        // Assert
        Assert.Equal("\"child:new_value,parent:undefined\"", result.ToFlatString());
    }

    [Fact]
    public void Maintain_Function_Resolution_Through_Environment_Chain()
    {
        // Arrange
        var parentEnv = EvaluationEnvironment.CreateStandard();
        parentEnv.BindFunction("parentFunc", typeof(TestHelpers).GetMethod(nameof(TestHelpers.RegularFunction))!);
        var childEnv = parentEnv.CreateChildForBlock();
        childEnv.BindFunction("childFunc", typeof(TestHelpers).GetMethod(nameof(TestHelpers.RegularFunction))!);
        
        // Act
        var parentFuncFromChild = childEnv.Lookup("parentFunc");
        var childFuncFromChild = childEnv.Lookup("childFunc");
        var childFuncFromParent = parentEnv.Lookup("childFunc");
        
        // Assert
        Assert.NotEqual(JTokenType.Undefined, parentFuncFromChild.Type);
        Assert.NotEqual(JTokenType.Undefined, childFuncFromChild.Type);
        Assert.Equal(JTokenType.Undefined, childFuncFromParent.Type);
    }

    [Fact]
    public void Support_Variable_Shadowing_In_Child_Environments()
    {
        // Arrange
        var parentEnv = EvaluationEnvironment.CreateStandard();
        parentEnv.BindValue("shadowVar", new JValue("parent_value"));
        var childEnv = parentEnv.CreateChildForBlock();
        childEnv.BindValue("shadowVar", new JValue("child_value"));
        
        // Act
        var parentValue = parentEnv.Lookup("shadowVar");
        var childValue = childEnv.Lookup("shadowVar");
        
        // Assert
        Assert.Equal("\"parent_value\"", parentValue.ToFlatString());
        Assert.Equal("\"child_value\"", childValue.ToFlatString());
    }

    [Fact]
    public void Allow_Custom_Functions_To_Receive_Current_Environment_When_Registered_With_MethodInfo()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindValue("testVar", new JValue("method_info_test"));
        var methodInfo = typeof(TestHelpers).GetMethod(nameof(TestHelpers.CaptureEnvironmentVariable));
        environment.BindFunction("captureVar", methodInfo!);
        var query = new JsonataQuery("$captureVar('testVar')", environment);
        
        // Act
        var result = query.Eval(JValue.CreateNull());
        
        // Assert
        Assert.Equal("\"method_info_test\"", result.ToFlatString());
    }

    [Fact]
    public void Allow_Custom_Functions_To_Receive_Current_Environment_When_Registered_With_Delegate()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindValue("testVar", new JValue("delegate_test"));
        Func<string, EvaluationEnvironment, JToken> delegateFunc = TestHelpers.CaptureEnvironmentVariable;
        environment.BindFunction("captureVar", delegateFunc);
        var query = new JsonataQuery("$captureVar('testVar')", environment);
        
        // Act
        var result = query.Eval(JValue.CreateNull());
        
        // Assert
        Assert.Equal("\"delegate_test\"", result.ToFlatString());
    }

    [Fact]
    public void Provide_Environment_Access_To_Custom_Functions_For_Variable_Resolution()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindValue("dynamicVar", new JValue("resolved_value"));
        environment.BindFunction("resolveVar", typeof(TestHelpers).GetMethod(nameof(TestHelpers.CaptureEnvironmentVariable))!);
        var query = new JsonataQuery("$resolveVar('dynamicVar')", environment);
        
        // Act
        var result = query.Eval(JValue.CreateNull());
        
        // Assert
        Assert.Equal("\"resolved_value\"", result.ToFlatString());
    }

    [Fact]
    public void Provide_Environment_Access_To_Custom_Functions_For_Function_Resolution()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindFunction("targetFunc", typeof(TestHelpers).GetMethod(nameof(TestHelpers.RegularFunction))!);
        environment.BindFunction("resolveFunc", typeof(TestHelpers).GetMethod(nameof(TestHelpers.ResolveFunctionFromEnvironment))!);
        var query = new JsonataQuery("$resolveFunc('targetFunc')", environment);
        
        // Act
        var result = query.Eval(JValue.CreateNull());
        
        // Assert
        Assert.Equal("\"found\"", result.ToFlatString());
    }
}