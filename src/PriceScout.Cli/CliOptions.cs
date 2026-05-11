namespace PriceScout.Cli;

public sealed record CliOptions(
    string SearchText,
    string OutputDirectory,
    string Country,
    string Language,
    string Currency,
    string OpenAiApiKey,
    string SystemPromptFile,
    bool Stream);
