using PriceScout.Cli.Domain;
using PriceScout.Cli.Processing;

namespace PriceScout.Tests;

public sealed class PriceStatisticsTests
{
    [Fact]
    public void Median_ReturnsZero_ForEmptyList()
    {
        Assert.Equal(0m, PriceStatistics.Median([]));
    }

    [Fact]
    public void Quartiles_ReturnExpectedValues_ForFourItems()
    {
        var values = new[] { 10m, 20m, 30m, 40m };
        var (q1, median, q3) = PriceStatistics.Quartiles(values);

        Assert.Equal(15m, q1);
        Assert.Equal(25m, median);
        Assert.Equal(35m, q3);
    }

    [Fact]
    public void Calculate_UsesPreferredCurrency_WhenAvailable()
    {
        var stats = PriceStatistics.Calculate(
            [
                CreateOffer(100m, "USD"),
                CreateOffer(120m, "USD"),
                CreateOffer(200m, "EUR")
            ],
            "USD");

        Assert.Equal("USD", stats.Currency);
        Assert.Equal(2, stats.KnownPriceCount);
        Assert.Equal(100m, stats.MinTotalPrice);
        Assert.Equal(120m, stats.MaxTotalPrice);
    }

    private static ProductOffer CreateOffer(decimal totalAmount, string currency) =>
        new()
        {
            Price = new PriceInfo
            {
                TotalAmount = totalAmount,
                Currency = currency
            }
        };
}
