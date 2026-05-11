# price-scout-ai-tool

`price-scout-ai-tool` is a minimal .NET 10 CLI that uses the OpenAI Responses API with hosted `web_search` to research product offers, classify real matches vs strange deviations, save a structured JSON report, and generate a human-readable Markdown report.

## Requirements

- .NET 10 SDK
- `OPENAI_API_KEY`

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
  --out ./reports
```

## Output

- `report.json` is the canonical machine-readable artifact.
- `report.md` is the human-readable summary artifact.

## Disclaimer

- Prices can be stale.
- Web search can miss stores.
- This report is a research aid, not a purchase guarantee.
