using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.SystemTextJson;
using ObjectParsingTestsData;
using Xunit;

namespace Jsonata.Net.Native.Tests.SystemTextJson;
public class JSONCheckerNode
{
    [Theory]
    [MemberData(nameof(JsonCheckerData.GetTestCasesXunit), MemberType = typeof(JsonCheckerData))]
    public void TestJsonNode(JsonCheckerData testCase)
    {
        if (testCase.expectedResult != true)
        {
            Exception exc = Assert.ThrowsAny<Exception>(() => JsonNode.Parse(testCase.json));
            Assert.IsType<JsonException>(exc);
        }
        else
        {
            JsonNode? node = JsonNode.Parse(testCase.json);
            JToken parsed = JsonataExtensions.FromSystemTextJson(node);

            JToken expected = JToken.Parse(testCase.json);
            string flatExpected = expected.ToFlatString();
            string flatParsed = parsed.ToFlatString();

            Assert.Equal(flatExpected, flatParsed);

            ////and back, see https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/use-dom#compare-jsonnode-options 
            //JsonNode? node2 = parsed.ToSystemTextJsonNode();
            //bool equal = JsonNode.DeepEquals(node, node2);
            //Assert.True(equal);

            //UPD: as it turned out the JsonNode.DeepEquals() is not adequate since it would fail on 1.0 == 1
        }
    }
}