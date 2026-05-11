namespace PriceScout.Cli;

internal static class App
{
    public static Task<int> RunAsync(string[] args)
    {
        var parseResult = CliParser.Parse(args);
        if (!parseResult.IsSuccess)
        {
            if (!string.IsNullOrWhiteSpace(parseResult.ErrorMessage))
            {
                Console.Error.WriteLine(parseResult.ErrorMessage);
            }

            if (parseResult.ShowUsage)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine(CliParser.GetUsageText());
            }

            return Task.FromResult(1);
        }

        return Task.FromResult(0);
    }
}
