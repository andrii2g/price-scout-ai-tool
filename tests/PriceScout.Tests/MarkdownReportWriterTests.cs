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

    [Fact]
    public void Write_IncludesRankedPriceSection_AndOfferDetails()
    {
        var report = CreateReport();
        report.RealMatches.Add(new ProductOffer
        {
            Id = "R1",
            Title = "Offer",
            Seller = "Seller",
            Condition = "new",
            Availability = "in_stock",
            Url = "https://example.com/product",
            Price = new PriceInfo { Amount = 9m, ShippingAmount = 1m, TotalAmount = 10m, Currency = "USD" },
            Match = new MatchInfo { Score = 0.9m, MatchedTerms = ["Brand"], MissingTerms = ["Model"], VariantNotes = "Exact variant" },
            Risk = new RiskInfo { Level = "low", Explanation = "Looks good" }
        });
        report.RealMatches.Add(new ProductOffer
        {
            Id = "R2",
            Title = "Offer 2",
            Seller = "Seller 2",
            Condition = "new",
            Availability = "in_stock",
            Url = "https://example.com/product-2",
            Price = new PriceInfo { Amount = 10m, ShippingAmount = 2m, TotalAmount = 12m, Currency = "USD" },
            Match = new MatchInfo { Score = 0.8m, MatchedTerms = ["Brand 2"], MissingTerms = ["Model 2"], VariantNotes = "Close variant" },
            Risk = new RiskInfo { Level = "medium", Explanation = "Review carefully" }
        });

        var markdown = MarkdownReportWriter.Write(report);

        Assert.Contains("## Ranked Real-Match Prices", markdown);
        Assert.Contains("| R1 | Offer | Seller |", markdown);
        Assert.Contains("| R2 | Offer 2 | Seller 2 |", markdown);
        Assert.Contains("### Real Match Details", markdown);
        Assert.Contains("- R1: Offer", markdown);
        Assert.Contains("- R2: Offer 2", markdown);
        Assert.Contains("Reasons: Looks good", markdown);
        Assert.Contains("Reasons: Review carefully", markdown);
        Assert.Contains("Matched terms: Brand", markdown);
        Assert.Contains("[https://example.com/product](https://example.com/product)", markdown);
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
