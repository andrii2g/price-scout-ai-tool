using System.Globalization;
using System.Text.Json;
using PriceScout.Cli.Domain;
using PriceScout.Cli.Processing;

namespace PriceScout.Cli.Reports;

public static class ReportFileWriter
{
    public static async Task<ReportWriteResult> WriteAsync(
        string outputDirectory,
        PriceScoutReport report,
        CliOptions options,
        CancellationToken cancellationToken)
    {
        var processed = ReportPostProcessor.Process(report, options);
        var timestamp = ResolveTimestamp(processed);
        var slug = SlugGenerator.Generate(options.SearchText);

        Directory.CreateDirectory(outputDirectory);

        var (jsonPath, markdownPath) = ResolveUniquePaths(outputDirectory, timestamp, slug);

        var json = JsonSerializer.Serialize(processed, CreateJsonOptions());
        var markdown = MarkdownReportWriter.Write(processed);

        await File.WriteAllTextAsync(jsonPath, json, cancellationToken);
        await File.WriteAllTextAsync(markdownPath, markdown, cancellationToken);
        return new ReportWriteResult(jsonPath, markdownPath);
    }

    private static string ResolveTimestamp(PriceScoutReport report)
    {
        if (DateTimeOffset.TryParse(report.Run.GeneratedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed.UtcDateTime.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture);
        }

        return DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture);
    }

    private static (string jsonPath, string markdownPath) ResolveUniquePaths(string outputDirectory, string timestamp, string slug)
    {
        var baseName = $"{timestamp}_{slug}";
        var attempt = 1;

        while (true)
        {
            var suffix = attempt == 1 ? string.Empty : $"-{attempt}";
            var jsonPath = Path.Combine(outputDirectory, $"{baseName}{suffix}.json");
            var markdownPath = Path.Combine(outputDirectory, $"{baseName}{suffix}.md");

            if (!File.Exists(jsonPath) && !File.Exists(markdownPath))
            {
                return (jsonPath, markdownPath);
            }

            attempt++;
        }
    }

    private static JsonSerializerOptions CreateJsonOptions() =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };
}
