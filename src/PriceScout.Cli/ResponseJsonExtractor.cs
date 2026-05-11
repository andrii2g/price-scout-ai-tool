using System.Text;
using System.Text.Json;

namespace PriceScout.Cli;

public static class ResponseJsonExtractor
{
    public static string ExtractOutputText(string rawResponseJson)
    {
        using var doc = JsonDocument.Parse(rawResponseJson);
        var root = doc.RootElement;

        if (root.TryGetProperty("status", out var status) &&
            status.GetString() is string statusValue &&
            !string.Equals(statusValue, "completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"OpenAI response status is '{statusValue}'.");
        }

        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("OpenAI response does not contain an output array.");
        }

        var builder = new StringBuilder();

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("type", out var itemType) || itemType.GetString() != "message")
            {
                continue;
            }

            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var part in content.EnumerateArray())
            {
                if (part.TryGetProperty("type", out var partType) &&
                    partType.GetString() == "output_text" &&
                    part.TryGetProperty("text", out var text))
                {
                    builder.Append(text.GetString());
                }
            }
        }

        var result = builder.ToString().Trim();
        if (string.IsNullOrEmpty(result))
        {
            throw new InvalidOperationException("Could not find output_text in OpenAI response.");
        }

        return result;
    }
}
