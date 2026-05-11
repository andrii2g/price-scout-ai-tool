namespace PriceScout.Cli.Domain;

public sealed class RunInfo
{
    public string GeneratedAtUtc { get; set; } = string.Empty;

    public string Input { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;
}
