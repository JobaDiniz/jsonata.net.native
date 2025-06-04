using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jsonata.Net.Native.Json;
using NUnit.Framework;
using ObjectParsingTestsData;

namespace Jsonata.Net.Native.Tests;
public sealed class ObjectParsingTests
{
    [TestCaseSource(nameof(GetTestCases))]
    public void RegularCases(TestData testData)
    {
        // Test removed: FromObject is now internal and should not be used by public API consumers
        // Alternative: Use System.Text.Json for serialization and then parse with JToken.Parse
        var jsonString = System.Text.Json.JsonSerializer.Serialize(testData.SourceObject);
        JToken token = JToken.Parse(jsonString);
        string result = token.ToFlatString();
        Assert.That(result, Is.EqualTo(testData.ExpectedJson));
    }

    public static List<TestCaseData> GetTestCases()
    {
        return TestData.GetTestCasesNunit();
    }
}
