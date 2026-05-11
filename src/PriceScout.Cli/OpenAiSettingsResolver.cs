using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace PriceScout.Cli;

public static class OpenAiSettingsResolver
{
    public static OpenAiSettings Resolve(CliOptions options)
    {
        var configuration = BuildConfiguration();

        var apiKey = FirstNonEmpty(
            options.OpenAiApiKey,
            Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
            configuration["OpenAI:ApiKey"]);

        var model = FirstNonEmpty(
            Environment.GetEnvironmentVariable("OPENAI_MODEL"),
            configuration["OpenAI:Model"],
            "gpt-5.5");

        var baseUrl = FirstNonEmpty(
            Environment.GetEnvironmentVariable("OPENAI_BASE_URL"),
            configuration["OpenAI:BaseUrl"],
            "https://api.openai.com");

        return new OpenAiSettings(apiKey, model, baseUrl);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

        if (Debugger.IsAttached)
        {
            builder.AddUserSecrets(typeof(App).Assembly, optional: true, reloadOnChange: false);
        }

        builder.AddEnvironmentVariables();
        return builder.Build();
    }

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(static x => !string.IsNullOrWhiteSpace(x))?.Trim() ?? string.Empty;
}
