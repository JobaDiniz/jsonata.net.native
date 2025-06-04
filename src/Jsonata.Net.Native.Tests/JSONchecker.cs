using System;
using System.Text.Json;
using Jsonata.Net.Native.Json;
using ObjectParsingTestsData;
using Xunit;

namespace Jsonata.Net.Native.Tests;

public class JSONChecker
{
    [Theory]
    [MemberData(nameof(JsonCheckerData.GetTestCasesXunit), MemberType = typeof(JsonCheckerData))]
    public void TestJsonDocument(JsonCheckerData testCase)
    {
        if (testCase.expectedResult != true)
        {
            Exception exc = Assert.ThrowsAny<Exception>(() => JsonDocument.Parse(testCase.json));
            Assert.IsType<JsonException>(exc);
        }
        else
        {
            JsonDocument doc = JsonDocument.Parse(testCase.json);
            JToken parsed = JsonataExtensions.FromSystemTextJson(doc);

            JToken expected = JToken.Parse(testCase.json);
            string flatExpected = expected.ToFlatString();
            string flatParsed = parsed.ToFlatString();

            Assert.Equal(flatExpected, flatParsed);
        }
    }
}