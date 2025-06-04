using System.Text.Json;
using Jsonata.Net.Native.Json;
using Xunit;

namespace Jsonata.Net.Native.Tests;

public sealed class JTokenShould
{
    [Fact]
    public void CreateFromJsonDocument()
    {
        // Arrange
        const string json = """{"name": "test", "value": 42, "items": [1, 2, 3]}""";
        using JsonDocument doc = JsonDocument.Parse(json);
        
        // Act
        JToken result = JToken.FromJsonDocument(doc);
        
        // Assert
        Assert.Equal(JTokenType.Object, result.Type);
        JObject obj = (JObject)result;
        Assert.Equal("test", (string)obj.Properties["name"]);
        Assert.Equal(42, (int)obj.Properties["value"]);
        Assert.Equal(JTokenType.Array, obj.Properties["items"].Type);
    }

    [Fact]
    public void CreateFromJsonElement()
    {
        // Arrange
        const string json = """[1, 2, 3]""";
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement element = doc.RootElement;
        
        // Act
        JToken result = JToken.FromJsonElement(element);
        
        // Assert
        Assert.Equal(JTokenType.Array, result.Type);
        JArray array = (JArray)result;
        Assert.Equal(3, array.Count);
        Assert.Equal(1, (int)array.ChildrenTokens[0]);
        Assert.Equal(2, (int)array.ChildrenTokens[1]);
        Assert.Equal(3, (int)array.ChildrenTokens[2]);
    }

    [Fact]
    public void Parse()
    {
        // Arrange
        const string json = """{"test": true, "number": 123, "array": [null, "string"]}""";
        
        // Act
        JToken result = JToken.Parse(json);
        
        // Assert
        Assert.Equal(JTokenType.Object, result.Type);
        JObject obj = (JObject)result;
        Assert.True((bool)obj.Properties["test"]);
        Assert.Equal(123, (int)obj.Properties["number"]);
        
        JArray array = (JArray)obj.Properties["array"];
        Assert.Equal(2, array.Count);
        Assert.Equal(JTokenType.Null, array.ChildrenTokens[0].Type);
        Assert.Equal("string", (string)array.ChildrenTokens[1]);
    }

    [Fact]
    public void ParseWithJsonDocumentOptions()
    {
        // Arrange
        const string jsonWithTrailingComma = """{"test": true,}""";
        var options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };
        
        // Act
        JToken result = JToken.Parse(jsonWithTrailingComma, options);
        
        // Assert
        Assert.Equal(JTokenType.Object, result.Type);
        JObject obj = (JObject)result;
        Assert.True((bool)obj.Properties["test"]);
    }

    [Fact]
    public void ParseShouldProduceEquivalentResultToFromJsonDocument()
    {
        // Arrange
        const string json = """{"test": true}""";
        
        // Act
        JToken fromParse = JToken.Parse(json);
        
        using JsonDocument doc = JsonDocument.Parse(json);
        JToken fromJsonDocument = JToken.FromJsonDocument(doc);
        
        // Assert
        Assert.True(JToken.DeepEquals(fromParse, fromJsonDocument));
    }
}