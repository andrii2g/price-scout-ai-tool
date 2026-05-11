using System.CommandLine;

namespace PriceScout.Cli;

public static class CliParser
{
    private const string DefaultOutputDirectory = "./reports";
    private const string DefaultLanguage = "en";

    private static readonly Option<string?> SearchOption = CreateOptionalStringOption("--search", "Product/item description to research.");
    private static readonly Option<string?> SearchAliasOption = CreateOptionalStringOption("--in", "Alias for --search.");
    private static readonly Option<string> OutputOption = CreateOptionWithDefault("--out", DefaultOutputDirectory, "Output directory.");
    private static readonly Option<string> CountryOption = CreateOptionWithDefault("--country", string.Empty, "Country hint such as UA, PL, DE, US.");
    private static readonly Option<string> LanguageOption = CreateOptionWithDefault("--language", DefaultLanguage, "Preferred language.");
    private static readonly Option<string> CurrencyOption = CreateOptionWithDefault("--currency", string.Empty, "Preferred currency such as UAH, EUR, USD.");
    private static readonly Option<string> OpenAiApiKeyOption = CreateOptionWithDefault("--openai-api-key", string.Empty, "OpenAI API key. Overrides config and environment.");
    private static readonly RootCommand RootCommand = CreateRootCommand();

    public static CliParseResult Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return Failure("No arguments were provided.", showUsage: true);
        }

        var parseResult = RootCommand.Parse(args);
        if (parseResult.Errors.Count > 0)
        {
            return Failure(parseResult.Errors[0].Message, showUsage: true);
        }

        if (args.Any(static arg => arg is "--help" or "-h"))
        {
            return Failure(null, showUsage: true);
        }

        var search = parseResult.GetValue(SearchOption);
        var searchAlias = parseResult.GetValue(SearchAliasOption);

        if (search is not null && searchAlias is not null && !string.Equals(search, searchAlias, StringComparison.Ordinal))
        {
            return Failure("Values for --search and --in must be identical when both are provided.", showUsage: true);
        }

        var effectiveSearch = search ?? searchAlias;
        if (string.IsNullOrWhiteSpace(effectiveSearch))
        {
            return Failure("A product description must be provided with --search or --in.", showUsage: true);
        }

        effectiveSearch = effectiveSearch.Trim();
        if (effectiveSearch.Length < 3)
        {
            return Failure("Product description must be at least 3 characters after trimming.", showUsage: true);
        }

        return new CliParseResult(
            IsSuccess: true,
            Options: new CliOptions(
                SearchText: effectiveSearch,
                OutputDirectory: NormalizePath(parseResult.GetValue(OutputOption), DefaultOutputDirectory),
                Country: NormalizeUpper(parseResult.GetValue(CountryOption)),
                Language: NormalizePath(parseResult.GetValue(LanguageOption), DefaultLanguage),
                Currency: NormalizeUpper(parseResult.GetValue(CurrencyOption)),
                OpenAiApiKey: parseResult.GetValue(OpenAiApiKeyOption)?.Trim() ?? string.Empty),
            ErrorMessage: null,
            ShowUsage: false);
    }

    public static string GetUsageText() =>
        """
        Usage:
          price-scout-ai-tool --search "<text>" [--out <path>] [--country <code>] [--language <code>] [--currency <code>] [--openai-api-key <key>]
          price-scout-ai-tool --in "<text>" [--out <path>] [--country <code>] [--language <code>] [--currency <code>] [--openai-api-key <key>]

        Options:
          --search <text>    Product/item description to research.
          --in <text>        Alias for --search.
          --out <path>       Output directory. Default: ./reports
          --country <code>   Country hint such as UA, PL, DE, US.
          --language <code>  Preferred language. Default: en
          --currency <code>  Preferred currency such as UAH, EUR, USD.
          --openai-api-key   OpenAI API key. Overrides config and environment.
          --help             Show usage information.
        """;

    private static RootCommand CreateRootCommand()
    {
        var command = new RootCommand("Generate JSON and Markdown price scouting reports from OpenAI research.");
        command.Options.Add(SearchOption);
        command.Options.Add(SearchAliasOption);
        command.Options.Add(OutputOption);
        command.Options.Add(CountryOption);
        command.Options.Add(LanguageOption);
        command.Options.Add(CurrencyOption);
        command.Options.Add(OpenAiApiKeyOption);
        return command;
    }

    private static Option<string> CreateOptionWithDefault(string name, string defaultValue, string description)
    {
        var option = new Option<string>(name)
        {
            Description = description
        };
        option.DefaultValueFactory = _ => defaultValue;
        return option;
    }

    private static Option<string?> CreateOptionalStringOption(string name, string description) =>
        new(name)
        {
            Description = description
        };

    private static string NormalizePath(string? value, string fallback)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? fallback : trimmed;
    }

    private static string NormalizeUpper(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

    private static CliParseResult Failure(string? errorMessage, bool showUsage) =>
        new(
            IsSuccess: false,
            Options: null,
            ErrorMessage: errorMessage,
            ShowUsage: showUsage);
}
