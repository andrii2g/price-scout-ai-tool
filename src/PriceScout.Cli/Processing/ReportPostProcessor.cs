using PriceScout.Cli.Domain;

namespace PriceScout.Cli.Processing;

public static class ReportPostProcessor
{
    public static PriceScoutReport Process(PriceScoutReport report, CliOptions options)
    {
        var processed = report ?? throw new ArgumentNullException(nameof(report));

        processed.SchemaVersion = string.IsNullOrWhiteSpace(processed.SchemaVersion) ? "1.0" : processed.SchemaVersion;
        processed.Run.Input = options.SearchText;
        processed.Run.Country = options.Country;
        processed.Run.Language = options.Language;
        processed.Run.Currency = options.Currency;

        NormalizeOffers(processed.RealMatches, "R");
        NormalizeOffers(processed.StrangeDeviations, "D");

        var warnings = new HashSet<string>(processed.Warnings, StringComparer.Ordinal);
        processed.PriceStats = PriceStatistics.Calculate(processed.RealMatches, options.Currency);

        if (processed.RealMatches.Count == 0)
        {
            warnings.Add("No real matches found.");
        }

        if (processed.PriceStats.KnownPriceCount < 3)
        {
            warnings.Add("Fewer than 3 known real-match prices were available.");
        }

        if (HasMixedCurrencies(processed.RealMatches))
        {
            warnings.Add("Mixed currencies detected in real-match prices.");
        }

        processed.Warnings = warnings.ToList();
        return processed;
    }

    private static void NormalizeOffers(List<ProductOffer> offers, string prefix)
    {
        for (var i = 0; i < offers.Count; i++)
        {
            var offer = offers[i];
            offer.Id = $"{prefix}{i + 1}";

            if (offer.Price.Amount > 0 && offer.Price.TotalAmount <= 0)
            {
                offer.Price.TotalAmount = offer.Price.Amount + Math.Max(offer.Price.ShippingAmount, 0);
            }

            if (string.IsNullOrWhiteSpace(offer.Price.Currency) && offer.Price.Amount == 0 && offer.Price.TotalAmount == 0)
            {
                offer.Price.Currency = string.Empty;
            }

            offer.Match.Score = Clamp01(offer.Match.Score);
            offer.Risk.Score = Clamp01(offer.Risk.Score);
        }
    }

    private static bool HasMixedCurrencies(IEnumerable<ProductOffer> offers) =>
        offers
            .Select(static x => x.Price.Currency)
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Skip(1)
            .Any();

    private static decimal Clamp01(decimal value) => Math.Clamp(value, 0m, 1m);
}
