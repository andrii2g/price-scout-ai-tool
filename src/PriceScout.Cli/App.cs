namespace PriceScout.Cli;

internal static class App
{
    public static Task<int> RunAsync(string[] args)
    {
        var parseResult = CliParser.Parse(args);
        return Task.FromResult(parseResult.IsSuccess ? 0 : 1);
    }
}
