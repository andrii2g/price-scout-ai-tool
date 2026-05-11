namespace PriceScout.Cli;

public sealed record OpenAiSettings(
    string ApiKey,
    string Model,
    string BaseUrl);
