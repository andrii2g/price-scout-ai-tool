using System.Text;
using PriceScout.Cli.Domain;

namespace PriceScout.Cli.Reports;

public static class MarkdownReportWriter
{
    public static string Write(PriceScoutReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();

        builder.AppendLine("# Price Scout Report");
        builder.AppendLine();
        builder.AppendLine("## Run");
        builder.AppendLine();
        builder.AppendLine($"- Generated: {Escape(report.Run.GeneratedAtUtc)}");
        builder.AppendLine($"- Input: {Escape(report.Run.Input)}");
        builder.AppendLine($"- Country: {Escape(report.Run.Country)}");
        builder.AppendLine($"- Language: {Escape(report.Run.Language)}");
        builder.AppendLine($"- Currency: {Escape(report.Run.Currency)}");
        builder.AppendLine();

        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Summary: {Escape(report.Run.Summary)}");
        builder.AppendLine($"- Real matches: {report.RealMatches.Count}");
        builder.AppendLine($"- Strange deviations: {report.StrangeDeviations.Count}");
        builder.AppendLine($"- Warnings: {report.Warnings.Count}");
        builder.AppendLine();

        builder.AppendLine("## Target");
        builder.AppendLine();
        builder.AppendLine($"- Name: {Escape(report.Target.NormalizedName)}");
        builder.AppendLine($"- Brand: {Escape(report.Target.Brand)}");
        builder.AppendLine($"- Model: {Escape(report.Target.Model)}");
        builder.AppendLine($"- Key attributes: {FormatList(report.Target.KeyAttributes)}");
        builder.AppendLine($"- Must-have terms: {FormatList(report.Target.MustHaveTerms)}");
        builder.AppendLine($"- Exclude signals: {FormatList(report.Target.ExcludeSignals)}");
        builder.AppendLine();

        builder.AppendLine("## Price Summary");
        builder.AppendLine();
        builder.AppendLine($"- Currency: {Escape(report.PriceStats.Currency)}");
        builder.AppendLine($"- Known prices: {report.PriceStats.KnownPriceCount}");
        builder.AppendLine($"- Min: {report.PriceStats.MinTotalPrice}");
        builder.AppendLine($"- Q1: {report.PriceStats.Q1TotalPrice}");
        builder.AppendLine($"- Median: {report.PriceStats.MedianTotalPrice}");
        builder.AppendLine($"- Q3: {report.PriceStats.Q3TotalPrice}");
        builder.AppendLine($"- Max: {report.PriceStats.MaxTotalPrice}");
        builder.AppendLine($"- IQR: {report.PriceStats.IqrTotalPrice}");
        builder.AppendLine();

        builder.AppendLine("## Ranked Real-Match Prices");
        builder.AppendLine();
        AppendPriceTable(builder, report.RealMatches);
        builder.AppendLine();

        builder.AppendLine("## Real Matches");
        builder.AppendLine();
        AppendOfferTable(builder, report.RealMatches, includeSignals: false);
        builder.AppendLine();

        builder.AppendLine("## Strange Deviations");
        builder.AppendLine();
        AppendOfferTable(builder, report.StrangeDeviations, includeSignals: true);
        builder.AppendLine();

        builder.AppendLine("## Sources");
        builder.AppendLine();
        if (report.Sources.Count == 0)
        {
            builder.AppendLine("- None");
        }
        else
        {
            foreach (var source in report.Sources)
            {
                builder.AppendLine($"- {Escape(source.Title)}: {FormatUrl(source.Url)}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Warnings");
        builder.AppendLine();
        if (report.Warnings.Count == 0)
        {
            builder.AppendLine("- None");
        }
        else
        {
            foreach (var warning in report.Warnings)
            {
                builder.AppendLine($"- {Escape(warning)}");
            }
        }

        return builder.ToString();
    }

    private static void AppendPriceTable(StringBuilder builder, List<ProductOffer> offers)
    {
        var pricedOffers = offers
            .Where(static x => x.Price.TotalAmount > 0)
            .OrderBy(static x => x.Price.TotalAmount)
            .ToList();

        if (pricedOffers.Count == 0)
        {
            builder.AppendLine("No known real-match prices available.");
            return;
        }

        builder.AppendLine("| Rank | ID | Title | Seller | Total | Currency | Match | Risk |");
        builder.AppendLine("| ---: | --- | --- | --- | ---: | --- | ---: | --- |");

        for (var i = 0; i < pricedOffers.Count; i++)
        {
            var offer = pricedOffers[i];
            builder.AppendLine(
                $"| {i + 1} | {Escape(offer.Id)} | {Escape(offer.Title)} | {Escape(offer.Seller)} | {offer.Price.TotalAmount} | {Escape(offer.Price.Currency)} | {offer.Match.Score} | {Escape(offer.Risk.Level)} |");
        }
    }

    private static void AppendOfferTable(StringBuilder builder, List<ProductOffer> offers, bool includeSignals)
    {
        if (offers.Count == 0)
        {
            builder.AppendLine("No entries.");
            return;
        }

        builder.AppendLine(includeSignals
            ? "| ID | Title | Seller | Condition | Availability | Price | Shipping | Total | Currency | Match | Risk | Signals | Link |"
            : "| ID | Title | Seller | Condition | Availability | Price | Shipping | Total | Currency | Match | Risk | Link |");
        builder.AppendLine(includeSignals
            ? "| --- | --- | --- | --- | --- | ---: | ---: | ---: | --- | ---: | --- | --- | --- |"
            : "| --- | --- | --- | --- | --- | ---: | ---: | ---: | --- | ---: | --- | --- |");

        foreach (var offer in offers)
        {
            var row =
                $"| {Escape(offer.Id)} | {Escape(offer.Title)} | {Escape(offer.Seller)} | {Escape(offer.Condition)} | {Escape(offer.Availability)} | {offer.Price.Amount} | {offer.Price.ShippingAmount} | {offer.Price.TotalAmount} | {Escape(offer.Price.Currency)} | {offer.Match.Score} | {Escape(offer.Risk.Level)} |";

            if (includeSignals)
            {
                row += $" {Escape(string.Join(", ", offer.Risk.Signals))} |";
            }

            row += $" {FormatUrl(offer.Url)} |";
            builder.AppendLine(row);
            builder.AppendLine();
            builder.AppendLine($"Reasons: {Escape(offer.Risk.Explanation)}");
            builder.AppendLine($"Matched terms: {FormatList(offer.Match.MatchedTerms)}");
            builder.AppendLine($"Missing terms: {FormatList(offer.Match.MissingTerms)}");
            builder.AppendLine($"Variant notes: {Escape(offer.Match.VariantNotes)}");
            builder.AppendLine();
        }
    }

    private static string FormatList(List<string> values) =>
        values.Count == 0 ? "None" : string.Join(", ", values.Select(Escape));

    private static string FormatUrl(string? value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            var escaped = Escape(uri.ToString());
            return $"[{escaped}]({escaped})";
        }

        return "n/a";
    }

    private static string Escape(string? value) =>
        string.IsNullOrEmpty(value)
            ? string.Empty
            : value
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("|", "\\|", StringComparison.Ordinal)
                .Replace("*", "\\*", StringComparison.Ordinal)
                .Replace("_", "\\_", StringComparison.Ordinal)
                .Replace("[", "\\[", StringComparison.Ordinal)
                .Replace("]", "\\]", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal);
}
