namespace PriceScout.Cli;

public static class CliParser
{
    public static CliParseResult Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return new CliParseResult(false, null, "No arguments were provided.", true);
        }

        return new CliParseResult(
            true,
            new CliOptions(
                SearchText: string.Empty,
                OutputDirectory: "./reports",
                Country: string.Empty,
                Language: "en",
                Currency: string.Empty),
            null,
            false);
    }
}
