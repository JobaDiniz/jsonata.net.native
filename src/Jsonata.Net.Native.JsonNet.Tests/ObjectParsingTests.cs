using System.Collections.Generic;
using System.Linq;
using Jsonata.Net.Native.Json;
using Xunit;
using ObjectParsingTestsData;

namespace Jsonata.Net.Native.JsonNet.Tests
{
    public sealed class ObjectParsingTests
    {
        [Theory, MemberData(nameof(GetTestCases))]
        public void RegularCases(TestData testData)
        {
            JToken token = JsonataExtensions.FromObjectViaNewtonsoft(testData.SourceObject);
            string result = token.ToFlatString();
            Assert.Equal(testData.ExpectedJson, result);
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            return TestData.GetTestCasesXunit();
        }
    }
}