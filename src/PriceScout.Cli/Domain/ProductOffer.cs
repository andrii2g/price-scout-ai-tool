namespace PriceScout.Cli.Domain;

public sealed class ProductOffer
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Seller { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string SourceTitle { get; set; } = string.Empty;

    public string Condition { get; set; } = string.Empty;

    public string Availability { get; set; } = string.Empty;

    public PriceInfo Price { get; set; } = new();

    public MatchInfo Match { get; set; } = new();

    public RiskInfo Risk { get; set; } = new();
}
