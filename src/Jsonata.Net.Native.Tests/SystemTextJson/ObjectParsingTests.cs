using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.SystemTextJson;
using ObjectParsingTestsData;
using Xunit;

namespace Jsonata.Net.Native.Tests.SystemTextJson
{
    public class ObjectParsingTests
    {
        [Theory]
        [MemberData(nameof(TestData.GetTestCasesXunit), MemberType = typeof(TestData))]
        public void TestObjectParsing(TestData testData)
        {
            JToken token = JsonataExtensions.FromObjectViaSystemTextJson(testData.SourceObject);
            string actualJson = token.ToFlatString();
            Assert.Equal(testData.ExpectedJson, actualJson);
        }
    }
}