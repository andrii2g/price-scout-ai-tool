namespace PriceScout.Cli;

public sealed record CliParseResult(
    bool IsSuccess,
    CliOptions? Options,
    string? ErrorMessage,
    bool ShowUsage);
