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
        var cancellationToken = TestContext.Current.CancellationToken;
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

            var options = new CliOptions("Example Product", tempDirectory, "UA", "en", "USD", string.Empty, string.Empty, false);
            var result = await ReportFileWriter.WriteAsync(tempDirectory, report, options, cancellationToken);

            Assert.True(File.Exists(result.JsonPath));
            Assert.True(File.Exists(result.MarkdownPath));

            var json = await File.ReadAllTextAsync(result.JsonPath, cancellationToken);
            var markdown = await File.ReadAllTextAsync(result.MarkdownPath, cancellationToken);

            Assert.Contains("\"schemaVersion\"", json);
            Assert.Contains("# Price Scout Report", markdown);

            using var doc = JsonDocument.Parse(json);
            Assert.Equal("Example Product", doc.RootElement.GetProperty("run").GetProperty("input").GetString());
            Assert.Equal("2026-05-12T00:00:00.0000000Z", doc.RootElement.GetProperty("run").GetProperty("generatedAtUtc").GetString());
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task WriteAsync_AppendsSuffix_WhenBaseNameAlreadyExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tempDirectory = Path.Combine(Path.GetTempPath(), "price-scout-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var options = new CliOptions("Example Product", tempDirectory, "UA", "en", "USD", string.Empty, string.Empty, false);

            var first = await ReportFileWriter.WriteAsync(tempDirectory, CreateReport("2026-05-12T00:00:00Z"), options, cancellationToken);
            var second = await ReportFileWriter.WriteAsync(tempDirectory, CreateReport("2026-05-12T00:00:00Z"), options, cancellationToken);

            Assert.NotEqual(first.JsonPath, second.JsonPath);
            Assert.EndsWith("-2.json", second.JsonPath, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith("-2.md", second.MarkdownPath, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    private static PriceScoutReport CreateReport(string generatedAtUtc) =>
        new()
        {
            Run = new RunInfo
            {
                GeneratedAtUtc = generatedAtUtc,
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
}
