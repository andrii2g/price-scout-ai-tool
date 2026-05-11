using PriceScout.Cli.Domain;

namespace PriceScout.Cli.Reports;

public static class ReportFileWriter
{
    public static Task WriteAsync(string outputDirectory, PriceScoutReport report, CancellationToken cancellationToken)
    {
        _ = outputDirectory;
        _ = report;
        _ = cancellationToken;
        throw new NotImplementedException();
    }
}
