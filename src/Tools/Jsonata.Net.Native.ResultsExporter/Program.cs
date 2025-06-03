using System;

namespace Jsonata.Net.Native.ResultsExporter;
public sealed class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: TestResultsExporter <testReportDirectory>");
            Environment.Exit(1);
        }

        try
        {
            string testReportDir = args[0];
            var generator = new TestResultsBadgeGenerator();
            generator.ProcessTestReport(testReportDir);
            Console.WriteLine($"Successfully processed test results from {testReportDir}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}