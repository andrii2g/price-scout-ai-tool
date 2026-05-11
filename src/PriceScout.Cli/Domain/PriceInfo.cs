namespace PriceScout.Cli.Domain;

public sealed class PriceInfo
{
    public string RawText { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal ShippingAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public bool IsApproximate { get; set; }
}
