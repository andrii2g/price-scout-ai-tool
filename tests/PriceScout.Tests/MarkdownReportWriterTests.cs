using PriceScout.Cli.Domain;
using PriceScout.Cli.Reports;

namespace PriceScout.Tests;

public sealed class MarkdownReportWriterTests
{
    [Fact]
    public void Write_RendersUnsafeUrlsAsNa()
    {
        var report = CreateReport();
        report.RealMatches.Add(new ProductOffer
        {
            Id = "R1",
            Title = "Offer",
            Seller = "Seller",
            Url = "javascript:alert(1)",
            Price = new PriceInfo { TotalAmount = 10m, Currency = "USD" },
            Match = new MatchInfo { Score = 0.9m },
            Risk = new RiskInfo { Level = "low" }
        });

        var markdown = MarkdownReportWriter.Write(report);

        Assert.Contains("n/a", markdown);
        Assert.DoesNotContain("javascript:alert(1)", markdown);
    }

    [Fact]
    public void Write_EscapesHtmlLikeText()
    {
        var report = CreateReport();
        report.Warnings.Add("<script>alert(1)</script>");

        var markdown = MarkdownReportWriter.Write(report);

        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", markdown);
    }

    private static PriceScoutReport CreateReport() =>
        new()
        {
            Run = new RunInfo
            {
                GeneratedAtUtc = "2026-05-11T20:00:00Z",
                Input = "test",
                Country = "UA",
                Language = "en",
                Currency = "USD",
                Summary = "summary"
            },
            Target = new TargetInfo
            {
                NormalizedName = "Target",
                Brand = "Brand",
                Model = "Model"
            },
            PriceStats = new PriceStats
            {
                Currency = "USD"
            }
        };
}
