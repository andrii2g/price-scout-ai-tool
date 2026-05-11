using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PriceScout.Cli;

public sealed class OpenAiResponsesClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<string> CreateReportAsync(
        CliOptions options,
        Action<string>? progress,
        CancellationToken cancellationToken)
    {
        var settings = OpenAiSettingsResolver.Resolve(options);
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key was not found. Provide --openai-api-key, set OPENAI_API_KEY, configure OpenAI:ApiKey in appsettings.json, or use user secrets while debugging.");
        }

        var payload = OpenAiRequestFactory.Create(options, settings);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{settings.BaseUrl.TrimEnd('/')}/v1/responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        request.Content = JsonContent.Create(payload);

        using var response = await _httpClient.SendAsync(
            request,
            options.Stream ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"OpenAI API request failed: {(int)response.StatusCode} {response.ReasonPhrase}{Environment.NewLine}{errorBody}");
        }

        if (!options.Stream)
        {
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await ReadStreamingResponseAsync(stream, progress, cancellationToken);
    }

    private static async Task<string> ReadStreamingResponseAsync(
        Stream stream,
        Action<string>? progress,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream);
        var eventName = string.Empty;
        var dataBuilder = new StringBuilder();
        var emittedOutputMessage = false;
        var accumulatedOutputLength = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            if (line.Length == 0)
            {
                if (dataBuilder.Length > 0)
                {
                    var payload = dataBuilder.ToString().TrimEnd();
                    var completed = HandleServerSentEvent(
                        eventName,
                        payload,
                        progress,
                        ref emittedOutputMessage,
                        ref accumulatedOutputLength);

                    if (completed is not null)
                    {
                        return completed;
                    }
                }

                eventName = string.Empty;
                dataBuilder.Clear();
                continue;
            }

            if (line.StartsWith("event:", StringComparison.Ordinal))
            {
                eventName = line["event:".Length..].Trim();
                continue;
            }

            if (line.StartsWith("data:", StringComparison.Ordinal))
            {
                dataBuilder.AppendLine(line["data:".Length..].TrimStart());
            }
        }

        if (dataBuilder.Length > 0)
        {
            var payload = dataBuilder.ToString().TrimEnd();
            var completed = HandleServerSentEvent(
                eventName,
                payload,
                progress,
                ref emittedOutputMessage,
                ref accumulatedOutputLength);

            if (completed is not null)
            {
                return completed;
            }
        }

        throw new InvalidOperationException("Streaming response ended before response.completed was received.");
    }

    private static string? HandleServerSentEvent(
        string declaredEventName,
        string payload,
        Action<string>? progress,
        ref bool emittedOutputMessage,
        ref int accumulatedOutputLength)
    {
        if (string.Equals(payload, "[DONE]", StringComparison.Ordinal))
        {
            return null;
        }

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;
        var eventType = root.TryGetProperty("type", out var typeElement)
            ? typeElement.GetString() ?? declaredEventName
            : declaredEventName;

        if (string.IsNullOrWhiteSpace(eventType))
        {
            return null;
        }

        switch (eventType)
        {
            case "response.created":
                progress?.Invoke("OpenAI response created.");
                return null;

            case "response.in_progress":
                progress?.Invoke("OpenAI response is in progress.");
                return null;

            case "response.output_text.delta":
                if (root.TryGetProperty("delta", out var deltaElement))
                {
                    accumulatedOutputLength += deltaElement.GetString()?.Length ?? 0;
                    if (!emittedOutputMessage)
                    {
                        progress?.Invoke("Receiving structured output...");
                        emittedOutputMessage = true;
                    }
                    else if (accumulatedOutputLength >= 512)
                    {
                        progress?.Invoke("Receiving more structured output...");
                        accumulatedOutputLength = 0;
                    }
                }

                return null;

            case "response.completed":
                progress?.Invoke("OpenAI response completed.");
                if (root.TryGetProperty("response", out var responseElement))
                {
                    return responseElement.GetRawText();
                }

                throw new InvalidOperationException("Streaming response.completed event did not include a response object.");

            case "response.failed":
            case "error":
                throw new InvalidOperationException($"OpenAI streaming response failed: {payload}");
        }

        if (eventType.Contains("web_search_call", StringComparison.Ordinal))
        {
            progress?.Invoke(DescribeWebSearchEvent(eventType));
        }

        return null;
    }

    private static string DescribeWebSearchEvent(string eventType)
    {
        if (eventType.EndsWith(".in_progress", StringComparison.Ordinal))
        {
            return "Web search is in progress.";
        }

        if (eventType.EndsWith(".completed", StringComparison.Ordinal))
        {
            return "Web search completed.";
        }

        return $"OpenAI event: {eventType}";
    }
}
