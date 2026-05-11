using PriceScout.Cli;

namespace PriceScout.Tests;

public sealed class ResponseJsonExtractorTests
{
    [Fact]
    public void ExtractOutputText_ConcatenatesMultipleOutputTextParts()
    {
        const string rawJson =
            """
            {
              "status": "completed",
              "output": [
                {
                  "type": "message",
                  "content": [
                    { "type": "output_text", "text": "{ \"a\":" },
                    { "type": "output_text", "text": " 1 }" }
                  ]
                }
              ]
            }
            """;

        var result = ResponseJsonExtractor.ExtractOutputText(rawJson);

        Assert.Equal("{ \"a\": 1 }", result);
    }

    [Fact]
    public void ExtractOutputText_ThrowsWhenNoOutputTextExists()
    {
        const string rawJson =
            """
            {
              "status": "completed",
              "output": [
                {
                  "type": "message",
                  "content": [
                    { "type": "refusal", "text": "nope" }
                  ]
                }
              ]
            }
            """;

        Assert.Throws<InvalidOperationException>(() => ResponseJsonExtractor.ExtractOutputText(rawJson));
    }
}
