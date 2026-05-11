using System.Globalization;
using System.Text.Json;
using PriceScout.Cli.Domain;
using PriceScout.Cli.Processing;

namespace PriceScout.Cli.Reports;

public static class ReportFileWriter
{
    public static async Task WriteAsync(
        string outputDirectory,
        PriceScoutReport report,
        CliOptions options,
        CancellationToken cancellationToken)
    {
        var processed = ReportPostProcessor.Process(report, options);
        var timestamp = ResolveTimestamp(processed);
        var slug = SlugGenerator.Generate(options.SearchText);
        var baseName = $"{timestamp}_{slug}";

        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.Combine(outputDirectory, $"{baseName}.json");
        var markdownPath = Path.Combine(outputDirectory, $"{baseName}.md");

        var json = JsonSerializer.Serialize(processed, CreateJsonOptions());
        var markdown = MarkdownReportWriter.Write(processed);

        await File.WriteAllTextAsync(jsonPath, json, cancellationToken);
        await File.WriteAllTextAsync(markdownPath, markdown, cancellationToken);
    }

    private static string ResolveTimestamp(PriceScoutReport report)
    {
        if (DateTimeOffset.TryParse(report.Run.GeneratedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed.UtcDateTime.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture);
        }

        return DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture);
    }

    private static JsonSerializerOptions CreateJsonOptions() =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };
}
