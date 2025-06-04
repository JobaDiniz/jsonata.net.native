using Xunit;
using Jsonata.Net.Native.Json;

namespace Jsonata.Net.Native.Tests;

public class StringFunctionsEvalShould
{
    [Fact]
    public void Use_Parent_Environment_For_Expression_Evaluation()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindValue("parentVar", new JValue("parent_value"));
        var query = new JsonataQuery("$eval('$parentVar')", environment);
        
        // Act
        var result = query.Eval(JValue.CreateNull());
        
        // Assert
        Assert.Equal("\"parent_value\"", result.ToFlatString());
    }

    [Fact]
    public void Access_Custom_Variables_From_Current_Environment()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindValue("customVar", new JValue(42));
        environment.BindValue("formula", new JValue("$customVar * 2"));
        var query = new JsonataQuery("$eval($formula)", environment);
        
        // Act
        var result = query.Eval(JValue.CreateNull());
        
        // Assert
        Assert.Equal("84", result.ToFlatString());
    }

    [Fact]
    public void Resolve_Custom_Functions_From_Environment_Hierarchy()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindFunction("myFunc", typeof(TestHelpers).GetMethod(nameof(TestHelpers.RegularFunction))!);
        var query = new JsonataQuery("$eval('$myFunc(\"test\")')", environment);
        
        // Act
        var result = query.Eval(JValue.CreateNull());
        
        // Assert
        Assert.Equal("\"regular_test\"", result.ToFlatString());
    }

    [Fact]
    public void Create_Proper_Query_Context_With_Inherited_Environment()
    {
        // Arrange
        var environment = EvaluationEnvironment.CreateStandard();
        environment.BindValue("baseVar", new JValue("base"));
        environment.BindValue("expression", new JValue("$baseVar & '_extended'"));
        var query = new JsonataQuery("$eval($expression)", environment);
        
        // Act
        var result = query.Eval(JValue.CreateNull());
        
        // Assert
        Assert.Equal("\"base_extended\"", result.ToFlatString());
    }
}