using System.Text.Json;
using System.Text.Json.Nodes;

namespace PriceScout.Cli;

public static class OpenAiRequestFactory
{
    public static object Create(CliOptions options)
    {
        var settings = OpenAiSettingsResolver.Resolve(options);
        return Create(options, settings);
    }

    public static object Create(CliOptions options, OpenAiSettings settings)
    {
        var tool = new JsonObject
        {
            ["type"] = "web_search",
            ["search_context_size"] = "medium",
            ["external_web_access"] = true
        };

        if (!string.IsNullOrWhiteSpace(options.Country) && options.Country.Length == 2)
        {
            tool["user_location"] = new JsonObject
            {
                ["type"] = "approximate",
                ["country"] = options.Country
            };
        }

        return new JsonObject
        {
            ["model"] = settings.Model,
            ["reasoning"] = new JsonObject
            {
                ["effort"] = "low"
            },
            ["tools"] = new JsonArray(tool),
            ["tool_choice"] = "required",
            ["include"] = new JsonArray("web_search_call.action.sources"),
            ["input"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "system",
                    ["content"] = PromptBuilder.BuildSystemPrompt(options)
                },
                new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = PromptBuilder.BuildUserPrompt(options)
                }
            },
            ["text"] = new JsonObject
            {
                ["format"] = new JsonObject
                {
                    ["type"] = "json_schema",
                    ["name"] = "price_scout_report",
                    ["strict"] = true,
                    ["schema"] = JsonNode.Parse(PriceScoutJsonSchema.GetJson())
                }
            }
        };
    }
}
