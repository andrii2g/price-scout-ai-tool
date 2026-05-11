namespace PriceScout.Cli.Domain;

public sealed class MatchInfo
{
    public decimal Score { get; set; }

    public string Label { get; set; } = string.Empty;

    public List<string> MatchedTerms { get; set; } = [];

    public List<string> MissingTerms { get; set; } = [];

    public string VariantNotes { get; set; } = string.Empty;
}
