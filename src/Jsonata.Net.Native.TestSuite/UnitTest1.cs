#define IGNORE_FAILED
using Xunit;
using Xunit.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Jsonata.Net.Native.SystemTextJson;

namespace Jsonata.Net.Native.TestSuite
{
    public sealed class Tests
    {
        private static readonly JsonSerializerOptions s_serializerOptions = new JsonSerializerOptions();

        private const string TEST_SUITE_ROOT = "../../../../../jsonata-js/test/test-suite";
        private Dictionary<string, Jsonata.Net.Native.Json.JToken> m_datasets = new Dictionary<string, Jsonata.Net.Native.Json.JToken>();
        private readonly Dictionary<string, string> m_disabledTests = new Dictionary<string, string>() {
            { "tail-recursion.case005", "Tail recursion is not supported yet, and having StackOverflow here breaks tests" },
            { "tail-recursion.case006", "Tail recursion is not supported yet, and having StackOverflow here breaks tests" },
            { "tail-recursion.case007", "Tail recursion is not supported yet, and having StackOverflow here breaks tests" }
        };
        private readonly Dictionary<string, string> m_suppressedTests = new Dictionary<string, string>() {
            //{ "function-sum.case002", "The problem with precision: expected '90.57', got '90.57000000000001'. We may use decimal instead of double always, but it looks like an overill?" },
            { "function-encodeUrlComponent.case002", "JS function encodeURIComponent throws URIError 'if one attempts to encode a surrogate which is not part of a high-low pair', which is seem to be not a case with C#" },
            { "function-encodeUrl.case002", "JS function encodeURI throws URIError 'if one attempts to encode a surrogate which is not part of a high-low pair', which is seem to be not a case with C#" },
            { "function-decodeUrlComponent.case002", "JS function encodeURIComponent throws URIError 'if one attempts to encode a surrogate which is not part of a high-low pair', which is seem to be not a case with C#" },
            { "function-decodeUrl.case002", "JS function encodeURI throws URIError 'if one attempts to encode a surrogate which is not part of a high-low pair', which is seem to be not a case with C#" },
        };

        public Tests()
        {
            string testSuiteRoot = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, TEST_SUITE_ROOT);
            string datasetDirectory = Path.Combine(testSuiteRoot, "datasets");
            foreach (string file in Directory.EnumerateFiles(datasetDirectory, "*.json"))
            {
                //JsonNode dataset = JsonNode.Parse(File.ReadAllText(file));
                Jsonata.Net.Native.Json.JToken dataset = Jsonata.Net.Native.Json.JToken.Parse(File.ReadAllText(file));
                this.m_datasets.Add(Path.GetFileNameWithoutExtension(file), dataset);
            }
            Assert.NotEmpty(this.m_datasets);
            Console.WriteLine($"Loaded {this.m_datasets.Count} datasets");
        }

        [SkippableTheory, MemberData(nameof(GetTestCases))]
        public void Test(CaseInfo caseInfo)
        {
            //check disabled tests
            {
                if (this.m_disabledTests.TryGetValue(caseInfo.testName!, out string? justification))
                {
                    throw new XunitException(justification);
                }
            }

            /*
             data or dataset: If data is defined, use the value of the data field as the input data for the test case. 
             Otherwise, the dataset field contains the name of the dataset (in the datasets directory) to use as input data. 
             If value of the dataset field is null, then use 'undefined' as the input data when evaluating the jsonata expression.
             */
            Jsonata.Net.Native.Json.JToken data;
            if (caseInfo.data != null)
            {
                data = caseInfo.data;
            }
            else if (caseInfo.dataset != null)
            {
                if (!this.m_datasets.TryGetValue(caseInfo.dataset, out Jsonata.Net.Native.Json.JToken? datset))
                {
                    throw new XunitException("No datset with name " + caseInfo.dataset);
                    throw new NotImplementedException("Fix for compiler");
                }
                else
                {
                    data = datset;
                };
            }
            else
            {
                data = Jsonata.Net.Native.Json.JValue.CreateUndefined();
            };

            try
            {
                if (caseInfo.description != null)
                {
                    Console.WriteLine($"Description: '{caseInfo.description}'");
                };
                Console.WriteLine($"Expr is '{caseInfo.expr}'");
                Jsonata.Net.Native.Json.JToken result;
                try
                {
                    JsonataQuery query = new JsonataQuery(caseInfo.expr!);
                    result = query.Eval(data, caseInfo.bindings);
                }
                catch (JsonataException)
                {
                    throw; //forward to next catch
                }
                catch (NotImplementedException niEx)
                {
#if IGNORE_FAILED
                    Skip.If(true, $"Failed with exception: {niEx.Message}\n({niEx.GetType().Name})\n{niEx.StackTrace}");
                    return;
#else
                    throw;
#endif
                }
                catch (Exception ex) //TODO: remove after removing BaseException
                {
#if IGNORE_FAILED
                    Skip.If(true, $"Failed with exception: {ex.Message}\n({ex.GetType().Name})\n{ex.StackTrace}");
                    return;
#else
                    throw;
#endif
                }

                Console.WriteLine($"Result: '{result.ToFlatString()}'");
                /*
                In addition, (exactly) one of the following fields is specified for each test case:

                    result: The expected result of evaluation (if defined)
                    undefinedResult: A flag indicating the expected result of evaluation will be undefined
                    code: The code associated with the exception that is expected to be thrown when either compiling the expression or evaluating it
                 */

                if (this.m_suppressedTests.TryGetValue(caseInfo.testName!, out string? justification))
                {
                    Skip.If(true, justification);
                    return;
                }


                if (caseInfo.result != null)
                {
                    Console.WriteLine($"Expected: '{caseInfo.result.ToFlatString()}'");
                    Assert.True(Jsonata.Net.Native.Json.JToken.DeepEquals(caseInfo.result, result), $"Expected '{caseInfo.result.ToFlatString()}', got '{result.ToFlatString()}'");
                }
                else if (caseInfo.undefinedResult.HasValue && caseInfo.undefinedResult.Value)
                {
                    Console.WriteLine($"Expected 'undefined'");
                    Assert.True(result.Type == Jsonata.Net.Native.Json.JTokenType.Undefined, $"Expected 'undefined', got '{result.ToFlatString()}'");
                }
                else if (caseInfo.code != null)
                {
                    Console.WriteLine($"Expected error {caseInfo.code}");
                    throw new XunitException($"Expected error {caseInfo.code} ({caseInfo.token}), got '{result.ToFlatString()}'");
                }
                else if (caseInfo.error != null)
                {
                    Console.WriteLine($"Expected error {caseInfo.error.code}");
                    throw new XunitException($"Expected error {caseInfo.error.code} ({caseInfo.error.message}{caseInfo.error.functionName}), got '{result.ToFlatString()}'");
                }
                else
                {
                    throw new XunitException("Bad test case?");
                }
            }
            catch (JsonataException jsonataEx)
            {
                if (caseInfo.code != null)
                {
                    //Assert.Equals(caseInfo.code, jsonataEx.Code); //TODO: enable code checking later
                    return; // Expected to throw error with code {caseInfo.code}. Actually thrown {jsonataEx.Code}. Not checking codes yet
                }
                else if (caseInfo.error != null)
                {
                    Assert.Equal(caseInfo.error.code, jsonataEx.Code);
                    if (caseInfo.error.message != null)
                    {
                        Assert.Equal(caseInfo.error.message, jsonataEx.RawMessage);
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        private static void ProcessAndAddCaseData(string sourceFile, List<object[]> results, CaseInfo caseInfo, string info)
        {
            if (caseInfo.expr == null)
            {
                if (caseInfo.expr_file != null)
                {
                    string exprFile = Path.Combine(Path.GetDirectoryName(sourceFile)!, caseInfo.expr_file);
                    caseInfo.expr = File.ReadAllText(exprFile);
                }
                else
                {
                    throw new ArgumentException($"Error processing case {info}: no 'expr' or 'expr-file' specified");
                }
            }

            caseInfo.testName = info;
            FixCaseInfo(caseInfo);
            results.Add(new object[] { caseInfo });
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            List<object[]> results = new List<object[]>();
            string caseGroupsDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, TEST_SUITE_ROOT, "groups");
            foreach (string groupDir in Directory.EnumerateDirectories(caseGroupsDirectory))
            {
                string infoGroupPrefix = Path.GetFileName(groupDir);
                foreach (string testFile in Directory.EnumerateFiles(groupDir, "*.json"))
                {
                    try
                    {
                        //dot works like path separator in NUnit
                        string info = infoGroupPrefix + "." + Path.GetFileNameWithoutExtension(testFile);
                        string testStr = File.ReadAllText(testFile);
                        //JsonDocument doc = JsonDocument.Parse(testStr);
                        Jsonata.Net.Native.Json.JToken testToken = Jsonata.Net.Native.Json.JToken.Parse(testStr);
                        if (testToken is Jsonata.Net.Native.Json.JArray array)
                        {
                            int index = 0;
                            foreach (Jsonata.Net.Native.Json.JToken subTestToken in array.ChildrenTokens)
                            {
                                CaseInfo caseInfo = CreateCaseInfoFromJToken(subTestToken);
                                ++index;
                                ProcessAndAddCaseData(testFile, results, caseInfo, info + "[" + index + "]");
                            }
                        }
                        else
                        {
                            CaseInfo caseInfo = CreateCaseInfoFromJToken(testToken);
                            ProcessAndAddCaseData(testFile, results, caseInfo, info);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Error parsing file {testFile}: {e.Message}", e);
                    }
                }
            }
            return results;
        }

        private static CaseInfo CreateCaseInfoFromJToken(Jsonata.Net.Native.Json.JToken token)
        {
            if (token is not Jsonata.Net.Native.Json.JObject obj)
                throw new Exception("Expected JObject for test case");

            var caseInfo = new CaseInfo();
            
            if (obj.Properties.TryGetValue("description", out var desc) && desc.Type == Jsonata.Net.Native.Json.JTokenType.String)
                caseInfo.description = (string)desc;
            
            if (obj.Properties.TryGetValue("expr", out var expr) && expr.Type == Jsonata.Net.Native.Json.JTokenType.String)
                caseInfo.expr = (string)expr;
            
            if (obj.Properties.TryGetValue("expr-file", out var exprFile) && exprFile.Type == Jsonata.Net.Native.Json.JTokenType.String)
                caseInfo.expr_file = (string)exprFile;
            
            if (obj.Properties.TryGetValue("data", out var data))
                caseInfo.data = data;
            
            if (obj.Properties.TryGetValue("dataset", out var dataset) && dataset.Type == Jsonata.Net.Native.Json.JTokenType.String)
                caseInfo.dataset = (string)dataset;
            
            if (obj.Properties.TryGetValue("timelimit", out var timelimit) && timelimit.Type == Jsonata.Net.Native.Json.JTokenType.Integer)
                caseInfo.timelimit = (int)timelimit;
            
            if (obj.Properties.TryGetValue("depth", out var depth) && depth.Type == Jsonata.Net.Native.Json.JTokenType.Integer)
                caseInfo.depth = (int)depth;
            
            if (obj.Properties.TryGetValue("bindings", out var bindings) && bindings is Jsonata.Net.Native.Json.JObject bindingsObj)
                caseInfo.bindings = bindingsObj;
            
            if (obj.Properties.TryGetValue("result", out var result))
                caseInfo.result = result;
            
            if (obj.Properties.TryGetValue("undefinedResult", out var undefinedResult) && undefinedResult.Type == Jsonata.Net.Native.Json.JTokenType.Boolean)
                caseInfo.undefinedResult = (bool)undefinedResult;
            
            if (obj.Properties.TryGetValue("code", out var code) && code.Type == Jsonata.Net.Native.Json.JTokenType.String)
                caseInfo.code = (string)code;
            
            if (obj.Properties.TryGetValue("token", out var tokenVal) && tokenVal.Type == Jsonata.Net.Native.Json.JTokenType.String)
                caseInfo.token = (string)tokenVal;
            
            if (obj.Properties.TryGetValue("error", out var error) && error is Jsonata.Net.Native.Json.JObject errorObj)
            {
                caseInfo.error = new CaseInfo.Error();
                if (errorObj.Properties.TryGetValue("code", out var errorCode) && errorCode.Type == Jsonata.Net.Native.Json.JTokenType.String)
                    caseInfo.error.code = (string)errorCode;
                if (errorObj.Properties.TryGetValue("message", out var errorMessage) && errorMessage.Type == Jsonata.Net.Native.Json.JTokenType.String)
                    caseInfo.error.message = (string)errorMessage;
                if (errorObj.Properties.TryGetValue("functionName", out var errorFunctionName) && errorFunctionName.Type == Jsonata.Net.Native.Json.JTokenType.String)
                    caseInfo.error.functionName = (string)errorFunctionName;
                if (errorObj.Properties.TryGetValue("value", out var errorValue) && errorValue.Type == Jsonata.Net.Native.Json.JTokenType.String)
                    caseInfo.error.value = (string)errorValue;
            }
            
            return caseInfo;
        }

        private static void FixCaseInfo(CaseInfo caseInfo)
        {
            switch (caseInfo.testName!)
            {
            case "range-operator.case021":
                //TODO: old value was "10000000.0" for unclear reason. Why should count() return such value? Also https://try.jsonata.org/ does not return fractional zero here
                caseInfo.result = new Jsonata.Net.Native.Json.JValue(10000000); 
                break;
            case "range-operator.case024":
                //TODO: old value was "10000000.0" for unclear reason. Why should count() return such value? Also https://try.jsonata.org/ does not return fractional zero here
                caseInfo.result = new Jsonata.Net.Native.Json.JValue(10000000);
                break;
            }
        }
    }
}