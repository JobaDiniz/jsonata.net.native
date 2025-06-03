using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Jsonata.Net.Js;
using Jsonata.Net.Native.Json;
using System;
using System.Diagnostics;
using System.IO;

namespace BenchmarkApp;
[MemoryDiagnoser]
public class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<Program>();
    }

    private readonly string data;
    private readonly string query;
    private readonly JsonataEngine jsEngine;
    private readonly int m_iterations = 1;

    public Program()
    {
        Console.WriteLine(Directory.GetCurrentDirectory());
        this.data = File.ReadAllText("employees.json");
        this.query = @"
                {
                  'name': Employee.FirstName & ' ' & Employee.Surname,
                  'mobile': Contact.Phone[type = 'mobile'].number
                }
            ";

        this.jsEngine = new Jsonata.Net.Js.JsonataEngine();
    }

    [Benchmark]
    public void ProcessNative()
    {
        Jsonata.Net.Native.JsonataQuery query = new Jsonata.Net.Native.JsonataQuery(this.query);
        JToken json = JToken.Parse(this.data);
        for (int i = 0; i < this.m_iterations; ++i)
        {
            query.Eval(json);
        }
    }

    [Benchmark]
    public void ProcessJs()
    {
        for (int i = 0; i < this.m_iterations; ++i)
        {
            jsEngine.Execute(this.query, this.data);
        }
    }
}
