namespace PriceScout.Cli;

public sealed class OpenAiResponsesClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public Task<string> CreateReportAsync(CliOptions options, CancellationToken cancellationToken)
    {
        _ = options;
        _ = cancellationToken;
        _ = _httpClient;
        throw new NotImplementedException();
    }
}
