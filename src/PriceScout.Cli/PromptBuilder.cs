namespace PriceScout.Cli;

public static class PromptBuilder
{
    public static string BuildSystemPrompt(CliOptions options)
    {
        var path = options.SystemPromptFile;
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("System prompt file path was not provided.");
        }

        var prompt = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new InvalidOperationException($"System prompt file is empty: {path}");
        }

        return prompt.Trim();
    }

    public static string BuildUserPrompt(CliOptions options)
    {
        return
            $"""
             Target item description: {options.SearchText}

             Preferences:
             - Country/local market hint: {ValueOrNotProvided(options.Country)}
             - Language hint: {ValueOrDefault(options.Language, "en")}
             - Preferred currency: {ValueOrNotProvided(options.Currency)}

             Research requirements:
             1. Search the web for current offers from local stores and marketplaces first.
             2. Find at least 5 candidate offers if available; more is better up to about 15.
             3. Identify exact/likely real matches.
             4. Identify strange deviations and suspicious/bait-like offers.
             5. Extract price, shipping, total, seller, condition, availability, source URL, and reason for classification.
             6. Fill priceStats using realMatches only when possible.
             7. Include warnings when the web search results are sparse, prices are unavailable, or confidence is low.
             """;
    }

    private static string ValueOrNotProvided(string value) =>
        string.IsNullOrWhiteSpace(value) ? "not provided" : value;

    private static string ValueOrDefault(string value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;
}
