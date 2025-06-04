using Xunit;
using Jsonata.Net.Native.Json;

namespace Jsonata.Net.Native.Tests;

public class StringFunctionsReplaceShould
{
    [Fact]
    public void Use_Current_Environment_For_Replacement_Function_Execution()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindValue("prefix", new JValue("NUM"));
        var query = new JsonataQuery("$replace('test 123', /[0-9]+/, function($match){ $prefix & '[' & $match.match & ']' })", environment);
        
        // Act
        var result = query.Eval(JValue.CreateNull());
        
        // Assert
        Assert.Equal("\"test NUM[123]\"", result.ToFlatString());
    }

    [Fact]
    public void Create_Child_Environment_For_Function_Replacements()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindValue("outer", new JValue("outer_value"));
        var query = new JsonataQuery("$replace('test', /test/, function($match){ $outer })", environment);
        
        // Act
        var result = query.Eval(JValue.CreateNull());
        
        // Assert
        Assert.Equal("\"outer_value\"", result.ToFlatString());
    }

    [Fact]
    public void Access_Variables_From_Parent_Environment_In_Replacement_Functions()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindValue("multiplier", new JValue(3));
        var query = new JsonataQuery("$replace('value 10', /[0-9]+/, function($match){ $number($match.match) * $multiplier })", environment);
        
        // Act
        var result = query.Eval(JValue.CreateNull());
        
        // Assert
        Assert.Equal("\"value 30\"", result.ToFlatString());
    }

    [Fact]
    public void Resolve_Custom_Functions_From_Environment_Chain()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindFunction("customFunc", typeof(TestHelpers).GetMethod(nameof(TestHelpers.RegularFunction))!);
        var query = new JsonataQuery("$replace('hello world', /world/, function($match){ $customFunc('replaced') })", environment);
        
        // Act
        var result = query.Eval(JValue.CreateNull());
        
        // Assert
        Assert.Equal("\"hello regular_replaced\"", result.ToFlatString());
    }
}