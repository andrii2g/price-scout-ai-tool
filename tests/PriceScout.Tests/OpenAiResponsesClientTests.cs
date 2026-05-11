using System.Net;
using System.Net.Http.Headers;
using System.Text;
using PriceScout.Cli;

namespace PriceScout.Tests;

public sealed class OpenAiResponsesClientTests
{
    [Fact]
    public async Task CreateReportAsync_ParsesStreamingResponse_AndReportsProgress()
    {
        var promptFile = CreatePromptFile("test system prompt");
        var progress = new List<string>();

        try
        {
            var sse = """
                event: response.created
                data: {"type":"response.created"}

                event: response.web_search_call.in_progress
                data: {"type":"response.web_search_call.in_progress"}

                event: response.output_text.delta
                data: {"type":"response.output_text.delta","delta":"{\"run\":"}

                event: response.completed
                data: {"type":"response.completed","response":{"status":"completed","output":[{"type":"message","content":[{"type":"output_text","text":"{\"run\":{}}"}]}]}}

                """;

            using var httpClient = new HttpClient(new StubHttpMessageHandler(sse))
            {
                BaseAddress = new Uri("https://api.openai.com")
            };

            var client = new OpenAiResponsesClient(httpClient);
            var options = new CliOptions(
                "item",
                "./reports",
                "UA",
                "en",
                "USD",
                "test-key",
                promptFile,
                true);

            var responseJson = await client.CreateReportAsync(
                options,
                progress.Add,
                TestContext.Current.CancellationToken);

            Assert.Contains("\"status\":\"completed\"", responseJson);
            Assert.Contains("OpenAI response created.", progress);
            Assert.Contains("Web search is in progress.", progress);
            Assert.Contains("Receiving structured output...", progress);
            Assert.Contains("OpenAI response completed.", progress);
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

    private sealed class StubHttpMessageHandler(string sseContent) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(sseContent, Encoding.UTF8, "text/event-stream")
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/event-stream");
            return Task.FromResult(response);
        }
    }
}
