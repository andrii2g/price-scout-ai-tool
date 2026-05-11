using PriceScout.Cli.Processing;

namespace PriceScout.Tests;

public sealed class SlugGeneratorTests
{
    [Fact]
    public void Generate_RemovesPunctuationAndNormalizesCase()
    {
        var slug = SlugGenerator.Generate("  Logitech!!! MX Master 3S  ");
        Assert.Equal("logitech-mx-master-3s", slug);
    }

    [Fact]
    public void Generate_FallsBackToReport_WhenNoAsciiRemains()
    {
        var slug = SlugGenerator.Generate("тест");
        Assert.Equal("report", slug);
    }
}
