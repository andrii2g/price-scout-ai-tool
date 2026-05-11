# price-scout-ai-tool

`price-scout-ai-tool` is a minimal .NET 10 CLI that uses the OpenAI Responses API with hosted `web_search` to research product offers, classify real matches vs strange deviations, save a structured JSON report, and generate a human-readable Markdown report.

## Requirements

- .NET 10 SDK
- OpenAI API key from one of:
  - `--openai-api-key`
  - `OPENAI_API_KEY`
  - `src/PriceScout.Cli/appsettings.json` (`OpenAI:ApiKey`)
  - user secrets while debugging

## Build

```bash
dotnet build src/PriceScout.Cli/PriceScout.Cli.csproj --no-restore
```

## Run

```bash
dotnet run --project src/PriceScout.Cli -- \
  --search "SONOFF Zigbee 3.0 USB Dongle Plus E" \
  --country UA \
  --language uk \
  --currency UAH \
  --out ./reports \
  --system-prompt-file ./system-prompts/general-item-search.txt \
  --stream
```

If you want the CLI to feel more responsive, pass:

```bash
--stream
```

That enables server-sent events from the Responses API and shows progress messages for response creation, web search activity, structured-output generation, and completion. It does not expose private chain-of-thought.

## Output

- `report.json` is the canonical machine-readable artifact.
- `report.md` is the human-readable summary artifact.

## Disclaimer

- Prices can be stale.
- Web search can miss stores.
- This report is a research aid, not a purchase guarantee.
