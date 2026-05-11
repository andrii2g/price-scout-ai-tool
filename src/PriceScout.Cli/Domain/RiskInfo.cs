namespace PriceScout.Cli.Domain;

public sealed class RiskInfo
{
    public decimal Score { get; set; }

    public string Level { get; set; } = string.Empty;

    public List<string> Signals { get; set; } = [];

    public string Explanation { get; set; } = string.Empty;
}
