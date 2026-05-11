using System.Text.Json.Nodes;
using PriceScout.Cli;

namespace PriceScout.Tests;

public sealed class OpenAiRequestFactoryTests
{
    [Fact]
    public void Create_IncludesCountryLocation_WhenCountryIsTwoLetters()
    {
        var payload = (JsonObject)OpenAiRequestFactory.Create(
            new CliOptions("item", "./reports", "UA", "uk", "UAH"));

        var tools = payload["tools"]!.AsArray();
        var tool = tools[0]!.AsObject();

        Assert.Equal("web_search", tool["type"]!.GetValue<string>());
        Assert.Equal("UA", tool["user_location"]!["country"]!.GetValue<string>());
    }

    [Fact]
    public void Create_OmitsCountryLocation_WhenCountryIsMissing()
    {
        var payload = (JsonObject)OpenAiRequestFactory.Create(
            new CliOptions("item", "./reports", string.Empty, "en", string.Empty));

        var tool = payload["tools"]!.AsArray()[0]!.AsObject();

        Assert.False(tool.ContainsKey("user_location"));
    }
}
