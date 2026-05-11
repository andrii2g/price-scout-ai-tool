using System.Text.Json.Nodes;
using PriceScout.Cli;

namespace PriceScout.Tests;

public sealed class OpenAiRequestFactoryTests
{
    [Fact]
    public void Create_IncludesCountryLocation_WhenCountryIsTwoLetters()
    {
        var payload = (JsonObject)OpenAiRequestFactory.Create(
            new CliOptions("item", "./reports", "UA", "uk", "UAH", string.Empty),
            new OpenAiSettings("key", "gpt-5.5", "https://api.openai.com"));

        var tools = payload["tools"]!.AsArray();
        var tool = tools[0]!.AsObject();

        Assert.Equal("web_search", tool["type"]!.GetValue<string>());
        Assert.Equal("UA", tool["user_location"]!["country"]!.GetValue<string>());
    }

    [Fact]
    public void Create_OmitsCountryLocation_WhenCountryIsMissing()
    {
        var payload = (JsonObject)OpenAiRequestFactory.Create(
            new CliOptions("item", "./reports", string.Empty, "en", string.Empty, string.Empty),
            new OpenAiSettings("key", "gpt-5.5", "https://api.openai.com"));

        var tool = payload["tools"]!.AsArray()[0]!.AsObject();

        Assert.False(tool.ContainsKey("user_location"));
    }

    [Fact]
    public void Create_UsesProvidedModel()
    {
        var payload = (JsonObject)OpenAiRequestFactory.Create(
            new CliOptions("item", "./reports", string.Empty, "en", string.Empty, string.Empty),
            new OpenAiSettings("key", "gpt-test", "https://api.openai.com"));

        Assert.Equal("gpt-test", payload["model"]!.GetValue<string>());
    }
}
