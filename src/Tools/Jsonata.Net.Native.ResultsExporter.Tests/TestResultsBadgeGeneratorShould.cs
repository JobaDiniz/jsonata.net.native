using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;
using Jsonata.Net.Native.ResultsExporter;
using VerifyXunit;
using System.Threading.Tasks;
using VerifyTests;

namespace Jsonata.Net.Native.ResultsExporter.Tests
{
    public class TestResultsBadgeGeneratorShould
    {
        private readonly string assetsDir;

        public TestResultsBadgeGeneratorShould()
        {
            assetsDir = Path.Combine(AppContext.BaseDirectory, "Assets");
        }

        [Fact]
        public async Task ShouldParseXmlAndGenerateExtractFile()
        {
            // Arrange
            var sut = new TestResultsBadgeGenerator();
            string xmlFile = Path.Combine(assetsDir, "sample-test-results.xml");
            string extractFile = Path.Combine(Path.GetTempPath(), $"extract-{Guid.NewGuid()}.txt");

            try
            {
                // Act
                sut.ExtractTestResults(xmlFile, extractFile);

                // Assert
                Assert.True(File.Exists(extractFile));
                string actualContent = await File.ReadAllTextAsync(extractFile);
                await Verify(actualContent).UseDirectory("_Snapshots");
            }
            finally
            {
                if (File.Exists(extractFile))
                    File.Delete(extractFile);
            }
        }

        [Fact]
        public void ShouldThrowWhenXmlFileNotFound()
        {
            // Arrange
            var sut = new TestResultsBadgeGenerator();
            string nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}.xml");
            string extractFile = Path.Combine(Path.GetTempPath(), $"extract-{Guid.NewGuid()}.txt");

            // Act & Assert
            var exception = Assert.Throws<FileNotFoundException>(() => 
                sut.ExtractTestResults(nonExistentFile, extractFile));
            
            Assert.Contains("Test log file not found", exception.Message);
        }

        [Fact]
        public async Task ShouldCreateCorrectBadgeFiles()
        {
            // Arrange
            var sut = new TestResultsBadgeGenerator();
            string extractFile = Path.Combine(assetsDir, "expected-extract.txt");
            string outputDir = Path.Combine(Path.GetTempPath(), $"output-{Guid.NewGuid()}");
            Directory.CreateDirectory(outputDir);

            try
            {
                // Act
                sut.GenerateBadgeJsonFiles(extractFile, outputDir);

                // Assert
                var badges = new Dictionary<string, object>();
                
                foreach (var fileName in new[] { "array-constructor.json", "function-abs.json", "conditional.json", "_all.json" })
                {
                    string actualFile = Path.Combine(outputDir, fileName);
                    Assert.True(File.Exists(actualFile), $"Badge file {fileName} was not created");
                    
                    string actualJson = await File.ReadAllTextAsync(actualFile);
                    var actualBadge = JsonSerializer.Deserialize<TestResultsBadgeGenerator.BadgeDescription>(actualJson);
                    
                    Assert.NotNull(actualBadge);
                    badges[Path.GetFileNameWithoutExtension(fileName)] = actualBadge;
                }

                await Verify(badges).UseDirectory("_Snapshots");
            }
            finally
            {
                if (Directory.Exists(outputDir))
                    Directory.Delete(outputDir, true);
            }
        }

        [Fact]
        public void ShouldThrowWhenExtractFileNotFound()
        {
            // Arrange
            var sut = new TestResultsBadgeGenerator();
            string nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}.txt");
            string outputDir = Path.Combine(Path.GetTempPath(), $"output-{Guid.NewGuid()}");

            // Act & Assert
            var exception = Assert.Throws<FileNotFoundException>(() => 
                sut.GenerateBadgeJsonFiles(nonExistentFile, outputDir));
            
            Assert.Contains("Extract file not found", exception.Message);
        }

        [Theory]
        [InlineData("all passed", "4 passed", "brightgreen")]
        [InlineData("all failed", "3 failed", "red")]
        [InlineData("all skipped", "2 skipped", "yellow")]
        [InlineData("mixed results", "2 passed | 1 failed | 1 skipped", "orange")]
        public async Task ShouldGenerateCorrectBadgeForTestResults(string label, string expectedMessage, string expectedColor)
        {
            // Arrange
            var sut = new TestResultsBadgeGenerator();
            var testResults = label switch
            {
                "all passed" => new[] { TestResultsBadgeGenerator.TestStatus.passed, TestResultsBadgeGenerator.TestStatus.passed, TestResultsBadgeGenerator.TestStatus.passed, TestResultsBadgeGenerator.TestStatus.passed },
                "all failed" => new[] { TestResultsBadgeGenerator.TestStatus.failed, TestResultsBadgeGenerator.TestStatus.failed, TestResultsBadgeGenerator.TestStatus.failed },
                "all skipped" => new[] { TestResultsBadgeGenerator.TestStatus.skipped, TestResultsBadgeGenerator.TestStatus.skipped },
                "mixed results" => new[] { TestResultsBadgeGenerator.TestStatus.passed, TestResultsBadgeGenerator.TestStatus.passed, TestResultsBadgeGenerator.TestStatus.failed, TestResultsBadgeGenerator.TestStatus.skipped },
                _ => throw new ArgumentException($"Unknown test case: {label}")
            };

            // Act
            var badge = sut.CreateBadgeDescription(label, testResults);

            // Assert
            Assert.Equal(1, badge.schemaVersion);
            Assert.Equal(label, badge.label);
            Assert.Equal(expectedMessage, badge.message);
            Assert.Equal(expectedColor, badge.color);
            
            await Verify(badge).UseDirectory("_Snapshots").UseParameters(label.Replace(" ", "_"));
        }

        [Fact]
        public async Task ShouldExecuteFullWorkflow()
        {
            // Arrange
            var sut = new TestResultsBadgeGenerator();
            string testReportDir = Path.Combine(Path.GetTempPath(), $"report-{Guid.NewGuid()}");
            Directory.CreateDirectory(testReportDir);
            
            string xmlFile = Path.Combine(testReportDir, "Jsonata.Net.Native.TestSuite.xml");
            string sourceXmlFile = Path.Combine(assetsDir, "sample-test-results.xml");
            
            // Copy the sample XML file to the expected location
            File.Copy(sourceXmlFile, xmlFile);

            try
            {
                // Act
                sut.ProcessTestReport(testReportDir);

                // Assert
                string extractFile = Path.Combine(testReportDir, "extract.txt");
                string extractDir = Path.Combine(testReportDir, "extract");
                
                Assert.True(File.Exists(extractFile));
                Assert.True(Directory.Exists(extractDir));
                
                // Verify some badge files were created
                Assert.True(File.Exists(Path.Combine(extractDir, "array-constructor.json")));
                Assert.True(File.Exists(Path.Combine(extractDir, "function-abs.json")));
                Assert.True(File.Exists(Path.Combine(extractDir, "conditional.json")));
                Assert.True(File.Exists(Path.Combine(extractDir, "_all.json")));

                // Verify the extract file content
                string extractContent = await File.ReadAllTextAsync(extractFile);
                await Verify(extractContent).UseDirectory("_Snapshots").UseMethodName("ShouldExecuteFullWorkflow_ExtractContent");
            }
            finally
            {
                if (Directory.Exists(testReportDir))
                    Directory.Delete(testReportDir, true);
            }
        }
    }
}