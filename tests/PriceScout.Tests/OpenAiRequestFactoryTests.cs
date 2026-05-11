using System.Text.Json.Nodes;
using PriceScout.Cli;

namespace PriceScout.Tests;

public sealed class OpenAiRequestFactoryTests
{
    [Fact]
    public void Create_IncludesCountryLocation_WhenCountryIsTwoLetters()
    {
        var promptFile = CreatePromptFile("test system prompt");

        try
        {
            var payload = (JsonObject)OpenAiRequestFactory.Create(
                new CliOptions("item", "./reports", "UA", "uk", "UAH", string.Empty, promptFile, false),
                new OpenAiSettings("key", "gpt-5.5", "https://api.openai.com"));

            var tools = payload["tools"]!.AsArray();
            var tool = tools[0]!.AsObject();

            Assert.Equal("web_search", tool["type"]!.GetValue<string>());
            Assert.Equal("UA", tool["user_location"]!["country"]!.GetValue<string>());
        }
        finally
        {
            File.Delete(promptFile);
        }
    }

    [Fact]
    public void Create_OmitsCountryLocation_WhenCountryIsMissing()
    {
        var promptFile = CreatePromptFile("test system prompt");

        try
        {
            var payload = (JsonObject)OpenAiRequestFactory.Create(
                new CliOptions("item", "./reports", string.Empty, "en", string.Empty, string.Empty, promptFile, false),
                new OpenAiSettings("key", "gpt-5.5", "https://api.openai.com"));

            var tool = payload["tools"]!.AsArray()[0]!.AsObject();

            Assert.False(tool.ContainsKey("user_location"));
        }
        finally
        {
            File.Delete(promptFile);
        }
    }

    [Fact]
    public void Create_UsesProvidedModel()
    {
        var promptFile = CreatePromptFile("test system prompt");

        try
        {
            var payload = (JsonObject)OpenAiRequestFactory.Create(
                new CliOptions("item", "./reports", string.Empty, "en", string.Empty, string.Empty, promptFile, false),
                new OpenAiSettings("key", "gpt-test", "https://api.openai.com"));

            Assert.Equal("gpt-test", payload["model"]!.GetValue<string>());
        }
        finally
        {
            File.Delete(promptFile);
        }
    }

    [Fact]
    public void Create_UsesSystemPromptFileContent()
    {
        var promptFile = CreatePromptFile("custom system prompt");

        try
        {
            var payload = (JsonObject)OpenAiRequestFactory.Create(
                new CliOptions("item", "./reports", string.Empty, "en", string.Empty, string.Empty, promptFile, false),
                new OpenAiSettings("key", "gpt-test", "https://api.openai.com"));

            var input = payload["input"]!.AsArray();
            var systemMessage = input[0]!.AsObject();

            Assert.Equal("custom system prompt", systemMessage["content"]!.GetValue<string>());
        }
        finally
        {
            File.Delete(promptFile);
        }
    }

    private static string CreatePromptFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"price-scout-prompt-{Guid.NewGuid():N}.txt");
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void Create_SetsStreamFlag_WhenRequested()
    {
        var promptFile = CreatePromptFile("test system prompt");

        try
        {
            var payload = (JsonObject)OpenAiRequestFactory.Create(
                new CliOptions("item", "./reports", string.Empty, "en", string.Empty, string.Empty, promptFile, true),
                new OpenAiSettings("key", "gpt-test", "https://api.openai.com"));

            Assert.True(payload["stream"]!.GetValue<bool>());
        }
        finally
        {
            File.Delete(promptFile);
        }
    }
}
