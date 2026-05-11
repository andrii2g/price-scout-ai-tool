namespace PriceScout.Cli.Domain;

public sealed class PriceStats
{
    public string Currency { get; set; } = string.Empty;

    public int KnownPriceCount { get; set; }

    public decimal MinTotalPrice { get; set; }

    public decimal Q1TotalPrice { get; set; }

    public decimal MedianTotalPrice { get; set; }

    public decimal Q3TotalPrice { get; set; }

    public decimal MaxTotalPrice { get; set; }

    public decimal IqrTotalPrice { get; set; }
}
