using PriceScout.Cli;

namespace PriceScout.Tests;

public sealed class CliParserTests
{
    [Fact]
    public void Parse_ValidMinimalSearch_UsesDefaults()
    {
        var result = CliParser.Parse(["--search", "abc"]);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Options);
        Assert.Equal("abc", result.Options.SearchText);
        Assert.Equal("./reports", result.Options.OutputDirectory);
        Assert.Equal("en", result.Options.Language);
        Assert.Equal(string.Empty, result.Options.Country);
        Assert.Equal(string.Empty, result.Options.Currency);
        Assert.Equal(string.Empty, result.Options.OpenAiApiKey);
    }

    [Fact]
    public void Parse_MismatchedAliases_Fails()
    {
        var result = CliParser.Parse(["--search", "abc", "--in", "xyz"]);

        Assert.False(result.IsSuccess);
        Assert.True(result.ShowUsage);
    }

    [Fact]
    public void Parse_UnknownOption_Fails()
    {
        var result = CliParser.Parse(["--search", "abc", "--unknown"]);

        Assert.False(result.IsSuccess);
        Assert.Contains("Unrecognized command or argument", result.ErrorMessage);
    }

    [Fact]
    public void Parse_ApiKeyOption_IsAccepted()
    {
        var result = CliParser.Parse(["--search", "abc", "--openai-api-key", "secret"]);

        Assert.True(result.IsSuccess);
        Assert.Equal("secret", result.Options!.OpenAiApiKey);
    }
}
