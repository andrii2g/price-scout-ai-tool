namespace PriceScout.Cli.Domain;

public sealed class PriceScoutReport
{
    public string SchemaVersion { get; set; } = string.Empty;

    public RunInfo Run { get; set; } = new();

    public TargetInfo Target { get; set; } = new();

    public PriceStats PriceStats { get; set; } = new();

    public List<ProductOffer> RealMatches { get; set; } = [];

    public List<ProductOffer> StrangeDeviations { get; set; } = [];

    public List<SourceInfo> Sources { get; set; } = [];

    public List<string> Warnings { get; set; } = [];
}
