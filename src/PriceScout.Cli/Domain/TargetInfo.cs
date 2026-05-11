namespace PriceScout.Cli.Domain;

public sealed class TargetInfo
{
    public string NormalizedName { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public List<string> KeyAttributes { get; set; } = [];

    public List<string> MustHaveTerms { get; set; } = [];

    public List<string> ExcludeSignals { get; set; } = [];
}
