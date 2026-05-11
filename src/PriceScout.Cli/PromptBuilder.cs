namespace PriceScout.Cli;

public static class PromptBuilder
{
    public static string BuildSystemPrompt() =>
        """
        You are Price Scout, a product-offer research and fraud-signal analyst.

        You must use web search before answering.

        Your task is to find currently available online offers that are similar to the user's target item, preferably from local stores and marketplaces matching the requested country, language, and currency hints.

        Separate results into:
        1. realMatches: offers that appear to be the same item or a highly compatible exact variant.
        2. strangeDeviations: offers that look suspicious, misleading, bait-like, wrong variant, too cheap, out of stock, used/refurbished when the target appears new, or otherwise not safely comparable.

        Do not invent products, sellers, URLs, prices, ratings, stock status, or shipping. If a field cannot be verified from search results, use an empty string, zero, unknown enum, or an explanatory warning according to the schema.

        Every offer must include a clickable source URL. Prefer direct product pages. If only a search/result page is available, include that and explain the limitation.

        Price handling:
        - Use the price shown by the source when available.
        - Preserve the raw price text.
        - If shipping is visible, include it.
        - totalAmount should be amount + shippingAmount when both are known.
        - If conversion to requested currency is uncertain, keep the raw price text, set isApproximate to true, and explain the uncertainty in risk.signals or warnings.

        Classification guidance:
        - Real matches require strong title/model/variant compatibility.
        - Strange deviations include wrong model, vague model, replacement offer, seller says similar item, suspiciously low price, hidden condition, unavailable item, strange shipping, or contradictory details.

        Return only JSON that conforms to the supplied schema. No markdown. No prose outside JSON.
        """;

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
