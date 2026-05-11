namespace PriceScout.Cli;

public static class CliParser
{
    private const string DefaultOutputDirectory = "./reports";
    private const string DefaultLanguage = "en";

    public static CliParseResult Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return Failure("No arguments were provided.", showUsage: true);
        }

        string? search = null;
        string? searchAlias = null;
        string outputDirectory = DefaultOutputDirectory;
        string country = string.Empty;
        string language = DefaultLanguage;
        string currency = string.Empty;
        string openAiApiKey = string.Empty;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--help":
                    return Failure(null, showUsage: true);
                case "--search":
                    if (!TryReadValue(args, ref i, out var searchValue))
                    {
                        return Failure("Missing value for --search.", showUsage: true);
                    }

                    search = searchValue;
                    break;
                case "--in":
                    if (!TryReadValue(args, ref i, out var inValue))
                    {
                        return Failure("Missing value for --in.", showUsage: true);
                    }

                    searchAlias = inValue;
                    break;
                case "--out":
                    if (!TryReadValue(args, ref i, out var outValue))
                    {
                        return Failure("Missing value for --out.", showUsage: true);
                    }

                    outputDirectory = outValue;
                    break;
                case "--country":
                    if (!TryReadValue(args, ref i, out var countryValue))
                    {
                        return Failure("Missing value for --country.", showUsage: true);
                    }

                    country = countryValue.ToUpperInvariant();
                    break;
                case "--language":
                    if (!TryReadValue(args, ref i, out var languageValue))
                    {
                        return Failure("Missing value for --language.", showUsage: true);
                    }

                    language = languageValue;
                    break;
                case "--currency":
                    if (!TryReadValue(args, ref i, out var currencyValue))
                    {
                        return Failure("Missing value for --currency.", showUsage: true);
                    }

                    currency = currencyValue.ToUpperInvariant();
                    break;
                case "--openai-api-key":
                    if (!TryReadValue(args, ref i, out var apiKeyValue))
                    {
                        return Failure("Missing value for --openai-api-key.", showUsage: true);
                    }

                    openAiApiKey = apiKeyValue;
                    break;
                default:
                    return Failure($"Unknown option: {args[i]}", showUsage: true);
            }
        }

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

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            outputDirectory = DefaultOutputDirectory;
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            language = DefaultLanguage;
        }

        return new CliParseResult(
            true,
            new CliOptions(
                SearchText: effectiveSearch,
                OutputDirectory: outputDirectory.Trim(),
                Country: country,
                Language: language.Trim(),
                Currency: currency,
                OpenAiApiKey: openAiApiKey),
            null,
            false);
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

    private static bool TryReadValue(string[] args, ref int index, out string value)
    {
        if (index + 1 >= args.Length)
        {
            value = string.Empty;
            return false;
        }

        value = args[++index];
        return true;
    }

    private static CliParseResult Failure(string? errorMessage, bool showUsage) =>
        new(
            IsSuccess: false,
            Options: null,
            ErrorMessage: errorMessage,
            ShowUsage: showUsage);
}
