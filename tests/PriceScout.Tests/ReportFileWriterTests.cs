using System.Text.Json;
using PriceScout.Cli;
using PriceScout.Cli.Domain;
using PriceScout.Cli.Reports;

namespace PriceScout.Tests;

public sealed class ReportFileWriterTests
{
    [Fact]
    public async Task WriteAsync_CreatesJsonAndMarkdownArtifacts()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "price-scout-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var report = new PriceScoutReport
            {
                Run = new RunInfo
                {
                    GeneratedAtUtc = "2026-05-12T00:00:00Z",
                    Input = "input",
                    Country = "UA",
                    Language = "en",
                    Currency = "USD",
                    Summary = "summary"
                },
                Target = new TargetInfo
                {
                    NormalizedName = "Target",
                    Brand = "Brand",
                    Model = "Model"
                }
            };

            var options = new CliOptions("Example Product", tempDirectory, "UA", "en", "USD");
            var result = await ReportFileWriter.WriteAsync(tempDirectory, report, options, CancellationToken.None);

            Assert.True(File.Exists(result.JsonPath));
            Assert.True(File.Exists(result.MarkdownPath));

            var json = await File.ReadAllTextAsync(result.JsonPath);
            var markdown = await File.ReadAllTextAsync(result.MarkdownPath);

            Assert.Contains("\"schemaVersion\"", json);
            Assert.Contains("# Price Scout Report", markdown);

            using var doc = JsonDocument.Parse(json);
            Assert.Equal("Example Product", doc.RootElement.GetProperty("run").GetProperty("input").GetString());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }
}
