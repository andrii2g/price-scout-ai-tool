using PriceScout.Cli.Domain;

namespace PriceScout.Cli.Processing;

public static class PriceStatistics
{
    public static PriceStats Calculate(IEnumerable<ProductOffer> realMatches, string preferredCurrency)
    {
        var matches = realMatches
            .Where(static x => x.Price.TotalAmount > 0)
            .ToList();

        if (matches.Count == 0)
        {
            return new PriceStats
            {
                Currency = preferredCurrency,
                KnownPriceCount = 0,
                MinTotalPrice = 0,
                Q1TotalPrice = 0,
                MedianTotalPrice = 0,
                Q3TotalPrice = 0,
                MaxTotalPrice = 0,
                IqrTotalPrice = 0
            };
        }

        var selectedMatches = SelectCurrencyMatches(matches, preferredCurrency);
        var sortedTotals = selectedMatches
            .Select(static x => x.Price.TotalAmount)
            .OrderBy(static x => x)
            .ToList();

        var (q1, median, q3) = Quartiles(sortedTotals);

        return new PriceStats
        {
            Currency = selectedMatches[0].Price.Currency,
            KnownPriceCount = sortedTotals.Count,
            MinTotalPrice = sortedTotals[0],
            Q1TotalPrice = q1,
            MedianTotalPrice = median,
            Q3TotalPrice = q3,
            MaxTotalPrice = sortedTotals[^1],
            IqrTotalPrice = q3 - q1
        };
    }

    private static List<ProductOffer> SelectCurrencyMatches(List<ProductOffer> matches, string preferredCurrency)
    {
        if (!string.IsNullOrWhiteSpace(preferredCurrency))
        {
            var preferred = matches
                .Where(x => string.Equals(x.Price.Currency, preferredCurrency, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (preferred.Count > 0)
            {
                return preferred;
            }
        }

        var dominantCurrency = matches
            .Where(x => !string.IsNullOrWhiteSpace(x.Price.Currency))
            .GroupBy(x => x.Price.Currency, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(static g => g.Count())
            .Select(static g => g.Key)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(dominantCurrency))
        {
            return matches;
        }

        return matches
            .Where(x => string.Equals(x.Price.Currency, dominantCurrency, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public static decimal Median(IReadOnlyList<decimal> sorted)
    {
        if (sorted.Count == 0)
        {
            return 0;
        }

        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2m
            : sorted[mid];
    }

    public static (decimal q1, decimal median, decimal q3) Quartiles(IReadOnlyList<decimal> sorted)
    {
        if (sorted.Count == 0)
        {
            return (0, 0, 0);
        }

        var median = Median(sorted);
        var mid = sorted.Count / 2;
        var lower = sorted.Take(mid).ToList();
        var upper = sorted.Count % 2 == 0
            ? sorted.Skip(mid).ToList()
            : sorted.Skip(mid + 1).ToList();

        var q1 = lower.Count == 0 ? sorted[0] : Median(lower);
        var q3 = upper.Count == 0 ? sorted[^1] : Median(upper);
        return (q1, median, q3);
    }
}
