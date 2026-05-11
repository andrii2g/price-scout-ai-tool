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
  --system-prompt-file ./system-prompts/general-item-search.txt
```

By default, reports are written to `./reports` relative to the directory you run the command from.

To always store reports in the repository-root `reports` folder, run the command from the repository root and pass:

```bash
--out ./reports
```

The default reusable system prompt template lives at:

```text
./system-prompts/general-item-search.txt
```

Copy that file to create item- or category-specific prompt variants, then pass the custom file with `--system-prompt-file`.

You can also pass the key explicitly:

```bash
dotnet run --project src/PriceScout.Cli -- \
  --search "SONOFF Zigbee 3.0 USB Dongle Plus E" \
  --openai-api-key "<your-key>"
```

For local debugging with user secrets:

```bash
dotnet user-secrets --project src/PriceScout.Cli set "OpenAI:ApiKey" "<your-key>"
```

## Output

- `report.json` is the canonical machine-readable artifact.
- `report.md` is the human-readable summary artifact.

## Disclaimer

- Prices can be stale.
- Web search can miss stores.
- This report is a research aid, not a purchase guarantee.
