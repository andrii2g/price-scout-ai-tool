using System.Text.Json;
using PriceScout.Cli.Domain;
using PriceScout.Cli.Reports;

namespace PriceScout.Cli;

internal static class App
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<int> RunAsync(string[] args)
    {
        var parseResult = CliParser.Parse(args);
        if (!parseResult.IsSuccess)
        {
            if (!string.IsNullOrWhiteSpace(parseResult.ErrorMessage))
            {
                Console.Error.WriteLine(parseResult.ErrorMessage);
            }

            if (parseResult.ShowUsage)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine(CliParser.GetUsageText());
            }

            return 2;
        }

        var options = parseResult.Options!;

        try
        {
            using var httpClient = new HttpClient();
            var openAiClient = new OpenAiResponsesClient(httpClient);
            var rawResponseJson = await openAiClient.CreateReportAsync(options, CancellationToken.None);
            var jsonText = ResponseJsonExtractor.ExtractOutputText(rawResponseJson);
            var report = JsonSerializer.Deserialize<PriceScoutReport>(jsonText, JsonOptions)
                ?? throw new InvalidOperationException("Structured report JSON was empty or invalid.");

            var result = await ReportFileWriter.WriteAsync(options.OutputDirectory, report, options, CancellationToken.None);

            Console.WriteLine($"Generated report JSON: {result.JsonPath}");
            Console.WriteLine($"Generated report Markdown: {result.MarkdownPath}");
            return 0;
        }
        catch (InvalidOperationException ex) when (
            ex.Message.Contains("OpenAI API key", StringComparison.Ordinal) ||
            ex.Message.Contains("OPENAI_API_KEY", StringComparison.Ordinal))
        {
            Console.Error.WriteLine(ex.Message);
            return 2;
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("OpenAI API request failed:", StringComparison.Ordinal))
        {
            Console.Error.WriteLine(ex.Message);
            return 3;
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"Structured report JSON was invalid: {ex.Message}");
            return 4;
        }
        catch (InvalidOperationException ex) when (
            ex.Message.Contains("output_text", StringComparison.Ordinal) ||
            ex.Message.Contains("Structured report JSON", StringComparison.Ordinal))
        {
            Console.Error.WriteLine(ex.Message);
            return 4;
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 5;
        }
    }
}
