using System;
using NUnit.Framework;

namespace Jsonata.Net.Native.Tests;

/// <summary>
/// Tests for JSONata query parsing and evaluation using the public string-based API.
/// These tests verify that JSONata expressions are parsed and evaluated correctly.
/// </summary>
public class QueryEvaluationTests
{
    [Test]
    public void TestVariableAssignmentAndComparison()
    {
        string query = "$x := $count($foo) > 0";
        JsonataQuery jsonataQuery = new JsonataQuery(query);
        string result = jsonataQuery.Eval("{}");
        Assert.AreEqual("false", result);
    }

    [Test]
    public void TestFactorialFunction()
    {
        string query = @"
                (
                  $factorial := function($x) {
                    $x <= 1 ? 1 : $x * $factorial($x-1)
                  };
                  $factorial(5)
                )             
            ";

        JsonataQuery jsonataQuery = new JsonataQuery(query);
        string result = jsonataQuery.Eval("{}");
        Assert.AreEqual("120", result);
    }
}
