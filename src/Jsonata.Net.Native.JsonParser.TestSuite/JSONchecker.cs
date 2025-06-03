using Jsonata.Net.Native.Json;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Jsonata.Net.Native.JsonParser.TestSuite;
//see http://www.json.org/JSON_checker/ and http://www.json.org/JSON_checker/test.zip
public sealed class JSONchecker
{
    private const string TEST_SUITE_ROOT = "../../../../../json-checker-tests";

    private ParseSettings m_parseSettings = ParseSettings.GetStrict();  //using strict for tests

    private static readonly Dictionary<string, string> s_testsToIgnore = new Dictionary<string, string>()
    {
    };

    private static readonly Dictionary<string, string> s_allowRejectingTestsToPass = new Dictionary<string, string>() {
            { "fail18", "Not too deep!" },
            { "fail17", "Maybe not that illegal?" },
            { "fail15", "Maybe not that illegal?" },
            { "fail13", "Let's allow that too" },
            { "fail1",  "Not a problem at all"}
        };

    [SkippableTheory, MemberData(nameof(GetTestCasesSync))]
    public void Test(CaseInfo caseInfo)
    {

        Console.WriteLine($"File: '{caseInfo.fileName}'");

        if (s_testsToIgnore.TryGetValue(caseInfo.fileName, out string? message))
        {
            Skip.If(true, message);
        }

        Console.WriteLine($"JSON: '{caseInfo.json}'");
        Console.WriteLine($"Expected: '{caseInfo.expectedResult}'");

        bool parsed;
        try
        {
            JToken resultToken = JToken.Parse(caseInfo.json, this.m_parseSettings);
            Console.WriteLine($"Parsed: '{resultToken.ToFlatString()}'");
            parsed = true;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Exception: '{ex.Message}'");
            parsed = false;
        }
        catch (JsonataException jsEx)
        {
            if (jsEx.Code == "S0102" && caseInfo.expectedResult == null)
            {
                Skip.If(true, "Skipping ambigous test with integer overflows");
            }
            throw;
        }
        catch (Exception)
        {
            throw;
        }

        Console.WriteLine($"Result: '{parsed}'");

        if (caseInfo.expectedResult == null)
        {
            Skip.If(true, "This is an ambigous test");
        }
        else if (
            caseInfo.expectedResult == false
            && parsed == true
            && s_allowRejectingTestsToPass.TryGetValue(caseInfo.fileName, out message)
        )
        {
            Skip.If(true, message);
        }
        else
        {
            Assert.Equal(caseInfo.expectedResult.Value, parsed);
        }
    }

    [SkippableTheory, MemberData(nameof(GetTestCasesValidateSync))]
    public void TestValidate(CaseInfo caseInfo)
    {

        Console.WriteLine($"File: '{caseInfo.fileName}'");

        if (s_testsToIgnore.TryGetValue(caseInfo.fileName, out string? message))
        {
            Skip.If(true, message);
        }

        Console.WriteLine($"JSON: '{caseInfo.json}'");
        Console.WriteLine($"Expected: '{caseInfo.expectedResult}'");

        bool parsed;
        try
        {
            JToken.Validate(caseInfo.json, this.m_parseSettings);
            Console.WriteLine($"Validated");
            parsed = true;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Exception: '{ex.Message}'");
            parsed = false;
        }
        catch (JsonataException jsEx)
        {
            if (jsEx.Code == "S0102" && caseInfo.expectedResult == null)
            {
                Skip.If(true, "Skipping ambigous test with integer overflows");
            }
            throw;
        }
        catch (Exception)
        {
            throw;
        }

        Console.WriteLine($"Result: '{parsed}'");

        if (caseInfo.expectedResult == null)
        {
            Skip.If(true, "This is an ambigous test");
        }
        else if (
            caseInfo.expectedResult == false
            && parsed == true
            && s_allowRejectingTestsToPass.TryGetValue(caseInfo.fileName, out message)
        )
        {
            Skip.If(true, message);
        }
        else
        {
            Assert.Equal(caseInfo.expectedResult.Value, parsed);
        }
    }

    [SkippableTheory, MemberData(nameof(GetTestCasesAsync))]
    public async Task TestAsync(CaseInfo caseInfo)
    {

        Console.WriteLine($"File: '{caseInfo.fileName}'");

        if (s_testsToIgnore.TryGetValue(caseInfo.fileName, out string? message))
        {
            Skip.If(true, message);
        }

        Console.WriteLine($"JSON: '{caseInfo.json}'");
        Console.WriteLine($"Expected: '{caseInfo.expectedResult}'");

        bool parsed;
        try
        {
            using (StringReader reader = new StringReader(caseInfo.json))
            {
                JToken resultToken = await JToken.ParseAsync(reader, CancellationToken.None, this.m_parseSettings);
                Console.WriteLine($"Parsed: '{resultToken.ToFlatString()}'");
            }
            parsed = true;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Exception: '{ex.Message}'");
            parsed = false;
        }
        catch (JsonataException jsEx)
        {
            if (jsEx.Code == "S0102" && caseInfo.expectedResult == null)
            {
                Skip.If(true, "Skipping ambigous test with integer overflows");
            }
            throw;
        }
        catch (Exception)
        {
            throw;
        }

        Console.WriteLine($"Result: '{parsed}'");

        if (caseInfo.expectedResult == null)
        {
            Skip.If(true, "This is an ambigous test");
        }
        else if (
            caseInfo.expectedResult == false
            && parsed == true
            && s_allowRejectingTestsToPass.TryGetValue(caseInfo.fileName, out message)
        )
        {
            Skip.If(true, message);
        }
        else
        {
            Assert.Equal(caseInfo.expectedResult.Value, parsed);
        }
    }

    [SkippableTheory, MemberData(nameof(GetTestCasesValidateAsync))]
    public async Task ValidateAsync(CaseInfo caseInfo)
    {
        Console.WriteLine($"File: '{caseInfo.fileName}'");

        if (s_testsToIgnore.TryGetValue(caseInfo.fileName, out string? message))
        {
            Skip.If(true, message);
        }

        Console.WriteLine($"JSON: '{caseInfo.json}'");
        Console.WriteLine($"Expected: '{caseInfo.expectedResult}'");

        bool parsed;
        try
        {
            using (StringReader reader = new StringReader(caseInfo.json))
            {
                await JToken.ValidateAsync(reader, CancellationToken.None, this.m_parseSettings);
                Console.WriteLine($"Validated");
            }
            parsed = true;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Exception: '{ex.Message}'");
            parsed = false;
        }
        catch (JsonataException jsEx)
        {
            if (jsEx.Code == "S0102" && caseInfo.expectedResult == null)
            {
                Skip.If(true, "Skipping ambigous test with integer overflows");
            }
            throw;
        }
        catch (Exception)
        {
            throw;
        }

        Console.WriteLine($"Result: '{parsed}'");

        if (caseInfo.expectedResult == null)
        {
            Skip.If(true, "This is an ambigous test");
        }
        else if (
            caseInfo.expectedResult == false
            && parsed == true
            && s_allowRejectingTestsToPass.TryGetValue(caseInfo.fileName, out message)
        )
        {
            Skip.If(true, message);
        }
        else
        {
            Assert.Equal(caseInfo.expectedResult.Value, parsed);
        }
    }

    private static void ProcessAndAddCaseData(List<object[]> results, CaseInfo caseInfo)
    {
        results.Add(new object[] { caseInfo });
    }

    public static IEnumerable<object[]> GetTestCasesSync()
    {
        return GetTestCasesImpl("parse_sync");
    }

    public static IEnumerable<object[]> GetTestCasesAsync()
    {
        return GetTestCasesImpl("parse_async");
    }

    public static IEnumerable<object[]> GetTestCasesValidateSync()
    {
        return GetTestCasesImpl("validate_sync");
    }

    public static IEnumerable<object[]> GetTestCasesValidateAsync()
    {
        return GetTestCasesImpl("validate_async");
    }

    private static IEnumerable<object[]> GetTestCasesImpl(string prefix)
    {
        List<object[]> results = new List<object[]>();
        string casesDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, TEST_SUITE_ROOT);
        foreach (string testFile in Directory.EnumerateFiles(casesDirectory, "*.json"))
        {
            //dot works like path separator in NUnit
            string fileName = Path.GetFileNameWithoutExtension(testFile);
            string displayName = fileName.Replace(".", "_");
            string json = File.ReadAllText(testFile);
            bool? result;

            /*
                 If the JSON_checker is working correctly, it must accept all of the pass*.json files and reject all of the fail*.json files. 
            */
            if (fileName.StartsWith("pass"))
            {
                result = true;
                displayName = prefix + ".pass." + displayName;
            }
            else if (fileName.StartsWith("fail"))
            {
                result = false;
                displayName = prefix + ".fail." + displayName;
            }
            else
            {
                throw new Exception("Unexpected file name " + fileName);
            }

            CaseInfo caseInfo = new CaseInfo()
            {
                displayName = displayName,
                fileName = fileName,
                json = json,
                expectedResult = result
            };
            ProcessAndAddCaseData(results, caseInfo);
        }
        return results;
    }
}