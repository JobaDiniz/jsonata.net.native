using System.Text.Json;
using System.Text.Json.Nodes;
using Jsonata.Net.Native.Json;
using Xunit;

namespace Jsonata.Net.Native.Tests;
public class UndefinedKindTests
{
    [Fact(Skip = "Creating JsonNode with undefined JsonElement is not supported in current System.Text.Json version")]
    public void CreateNodeWithUndefinedKind()
    {
        // This test is skipped because creating a JsonNode with an undefined JsonElement
        // throws InvalidOperationException in the current version of System.Text.Json
        JsonNode? node = JsonValue.Create(new JsonElement());
        Assert.NotNull(node);
        Assert.Equal(JsonValueKind.Undefined, node!.GetValueKind());

        JToken token = node.ToJToken();
        Assert.Equal(JTokenType.Undefined, token.Type);
    }

    [Fact]
    public void ConvertUndefinedToJsonNode()
    {
        JToken token = JValue.CreateUndefined();

        JsonNode? node = token.ToJsonNode();
        Assert.NotNull(node);
        Assert.Equal(JsonValueKind.Undefined, node!.GetValueKind());
    }
}