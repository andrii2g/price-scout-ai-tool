using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PriceScout.Cli;

public sealed class OpenAiResponsesClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<string> CreateReportAsync(CliOptions options, CancellationToken cancellationToken)
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

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"OpenAI API request failed: {(int)response.StatusCode} {response.ReasonPhrase}{Environment.NewLine}{body}");
        }

        return body;
    }
}
