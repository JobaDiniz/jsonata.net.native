using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Jsonata.Net.Native.Json;
using Xunit;
using ObjectParsingTestsData;

namespace Jsonata.Net.Native.SystemTextJson.Tests
{
    public sealed class UndefinedKindTests
    {
        [Fact]
        public void CreateNodeWithUndefinedKind()
        {
            JsonNode node = JsonValue.Create(new JsonElement())!;
            Assert.Equal(JsonValueKind.Undefined, node.GetValueKind());
        }

        [Fact]
        public void ConvertFromJTokenWithUndefined()
        {
            JToken jToken = JValue.CreateUndefined();
            JsonNode? node = jToken.ToSystemTextJsonNode();
            Assert.NotNull(node);
            Assert.Equal(JsonValueKind.Undefined, node!.GetValueKind());
        }

    }
}