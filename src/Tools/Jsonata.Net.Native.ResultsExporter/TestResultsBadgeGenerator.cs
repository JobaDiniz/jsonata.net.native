using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Jsonata.Net.Native.ResultsExporter
{
    internal sealed class TestResultsBadgeGenerator
    {
        public enum TestStatus
        {
            passed,
            failed,
            skipped
        }

        public sealed class BadgeDescription
        {
            public int schemaVersion { get; set; } = 1;
            public string label { get; set; } = "";
            public string message { get; set; } = "";
            public string color { get; set; } = "";
        }

        private static readonly Regex s_testCaseRegex = new Regex("^.* name=\"([^\"]+)\".* result=\"([^\"]+)\".*$", RegexOptions.Compiled);

        public void ProcessTestReport(string testReportDir)
        {
            string fullLogFile = Path.Combine(testReportDir, "Jsonata.Net.Native.TestSuite.xml");
            string extractFile = Path.Combine(testReportDir, "extract.txt");
            string jsonFilesDir = Path.Combine(testReportDir, "extract");

            ExtractTestResults(fullLogFile, extractFile);
            GenerateBadgeJsonFiles(extractFile, jsonFilesDir);
        }

        public void ExtractTestResults(string xmlLogFile, string extractFile)
        {
            if (!File.Exists(xmlLogFile))
            {
                throw new FileNotFoundException($"Test log file not found: {xmlLogFile}");
            }

            var extractedResults = File.ReadLines(xmlLogFile)
                .Where(line => line.Contains("<test-case"))
                .Select(line => s_testCaseRegex.Match(line))
                .Where(match => match.Success)
                .Select(match => match.Result("$1;$2"));

            File.WriteAllLines(extractFile, extractedResults);
        }

        public void GenerateBadgeJsonFiles(string extractFile, string outputDir)
        {
            if (!File.Exists(extractFile))
            {
                throw new FileNotFoundException($"Extract file not found: {extractFile}");
            }

            Directory.CreateDirectory(outputDir);

            var testGroups = File.ReadLines(extractFile)
                .Select(ParseTestResult)
                .GroupBy(result => result.TestGroup, result => result.Status)
                .ToList();

            // Generate individual group badges
            foreach (var testGroup in testGroups)
            {
                string outputFile = Path.Combine(outputDir, $"{testGroup.Key}.json");
                CreateBadgeFile(testGroup.Key, testGroup, outputFile);
            }

            // Generate overall summary badge
            string allTestsFile = Path.Combine(outputDir, "_all.json");
            CreateBadgeFile("all tests", testGroups.SelectMany(g => g), allTestsFile);
        }

        public void CreateBadgeFile(string label, IEnumerable<TestStatus> testResults, string outputFile)
        {
            var badge = CreateBadgeDescription(label, testResults);
            string json = JsonSerializer.Serialize(badge, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(outputFile, json);
        }

        public BadgeDescription CreateBadgeDescription(string label, IEnumerable<TestStatus> testResults)
        {
            var statusCounts = testResults
                .GroupBy(status => status)
                .ToDictionary(group => group.Key, group => group.Count());

            var badge = new BadgeDescription { label = label };
            badge.message = BuildStatusMessage(statusCounts);
            badge.color = DetermineColor(statusCounts);

            return badge;
        }

        private static (string TestGroup, TestStatus Status) ParseTestResult(string line)
        {
            string[] parts = line.Split(';');
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid test result format: {line}");
            }

            string testName = parts[0];
            string testGroup = testName.Substring(0, testName.IndexOf('.'));
            
            if (!Enum.TryParse<TestStatus>(parts[1].ToLower(), out TestStatus status))
            {
                throw new ArgumentException($"Invalid test status: {parts[1]}");
            }

            return (testGroup, status);
        }

        private static string BuildStatusMessage(Dictionary<TestStatus, int> statusCounts)
        {
            var messageBuilder = new StringBuilder();
            
            foreach (TestStatus status in Enum.GetValues<TestStatus>())
            {
                if (statusCounts.TryGetValue(status, out int count))
                {
                    if (messageBuilder.Length > 0)
                    {
                        messageBuilder.Append(" | ");
                    }
                    messageBuilder.Append($"{count} {status}");
                }
            }

            return messageBuilder.ToString();
        }

        private static string DetermineColor(Dictionary<TestStatus, int> statusCounts)
        {
            if (statusCounts.Count != 1)
            {
                return "orange"; // Mixed results
            }

            return statusCounts.Keys.First() switch
            {
                TestStatus.passed => "brightgreen",
                TestStatus.failed => "red",
                _ => "yellow"
            };
        }

        public void GenerateReadmeBadges(string jsonFilesDir, string outputFile)
        {
            const string style = "flat-square";

            var badgeMarkdown = Directory.EnumerateFiles(jsonFilesDir, "*.json")
                .Select(Path.GetFileName)
                .Where(fileName => fileName != null)
                .OrderBy(fileName => fileName)
                .Select(fileName => 
                {
                    string name = Path.GetFileNameWithoutExtension(fileName) ?? "";
                    string url = $"https://raw.githubusercontent.com/mikhail-barg/jsonata.net.native/master/src/Jsonata.Net.Native.TestSuite/TestReport/extract/{fileName}";
                    return $"* ![{name}](https://img.shields.io/endpoint?style={style}&url={WebUtility.UrlEncode(url)})";
                });

            File.WriteAllLines(outputFile, badgeMarkdown);
        }
    }
}