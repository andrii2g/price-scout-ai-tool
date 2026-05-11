using PriceScout.Cli.Domain;

namespace PriceScout.Cli.Processing;

public static class PriceStatistics
{
    public static PriceStats Calculate(IEnumerable<ProductOffer> realMatches, string preferredCurrency)
    {
        _ = realMatches;
        _ = preferredCurrency;
        throw new NotImplementedException();
    }
}
