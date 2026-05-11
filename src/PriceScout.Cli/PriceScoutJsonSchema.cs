namespace PriceScout.Cli;

public static class PriceScoutJsonSchema
{
    public static string GetJson() =>
        """
        {
          "type": "object",
          "additionalProperties": false,
          "required": [
            "schemaVersion",
            "run",
            "target",
            "priceStats",
            "realMatches",
            "strangeDeviations",
            "sources",
            "warnings"
          ],
          "properties": {
            "schemaVersion": { "type": "string" },
            "run": {
              "type": "object",
              "additionalProperties": false,
              "required": ["generatedAtUtc", "input", "country", "language", "currency", "summary"],
              "properties": {
                "generatedAtUtc": { "type": "string" },
                "input": { "type": "string" },
                "country": { "type": "string" },
                "language": { "type": "string" },
                "currency": { "type": "string" },
                "summary": { "type": "string" }
              }
            },
            "target": {
              "type": "object",
              "additionalProperties": false,
              "required": ["normalizedName", "brand", "model", "keyAttributes", "mustHaveTerms", "excludeSignals"],
              "properties": {
                "normalizedName": { "type": "string" },
                "brand": { "type": "string" },
                "model": { "type": "string" },
                "keyAttributes": { "type": "array", "items": { "type": "string" } },
                "mustHaveTerms": { "type": "array", "items": { "type": "string" } },
                "excludeSignals": { "type": "array", "items": { "type": "string" } }
              }
            },
            "priceStats": {
              "type": "object",
              "additionalProperties": false,
              "required": ["currency", "knownPriceCount", "minTotalPrice", "q1TotalPrice", "medianTotalPrice", "q3TotalPrice", "maxTotalPrice", "iqrTotalPrice"],
              "properties": {
                "currency": { "type": "string" },
                "knownPriceCount": { "type": "integer" },
                "minTotalPrice": { "type": "number" },
                "q1TotalPrice": { "type": "number" },
                "medianTotalPrice": { "type": "number" },
                "q3TotalPrice": { "type": "number" },
                "maxTotalPrice": { "type": "number" },
                "iqrTotalPrice": { "type": "number" }
              }
            },
            "realMatches": { "type": "array", "items": { "$ref": "#/$defs/offer" } },
            "strangeDeviations": { "type": "array", "items": { "$ref": "#/$defs/offer" } },
            "sources": { "type": "array", "items": { "$ref": "#/$defs/source" } },
            "warnings": { "type": "array", "items": { "type": "string" } }
          },
          "$defs": {
            "source": {
              "type": "object",
              "additionalProperties": false,
              "required": ["title", "url", "note"],
              "properties": {
                "title": { "type": "string" },
                "url": { "type": "string" },
                "note": { "type": "string" }
              }
            },
            "offer": {
              "type": "object",
              "additionalProperties": false,
              "required": ["id", "title", "seller", "url", "sourceTitle", "condition", "availability", "price", "match", "risk"],
              "properties": {
                "id": { "type": "string" },
                "title": { "type": "string" },
                "seller": { "type": "string" },
                "url": { "type": "string" },
                "sourceTitle": { "type": "string" },
                "condition": { "type": "string", "enum": ["new", "used", "refurbished", "open_box", "unknown"] },
                "availability": { "type": "string", "enum": ["in_stock", "out_of_stock", "preorder", "unknown"] },
                "price": {
                  "type": "object",
                  "additionalProperties": false,
                  "required": ["rawText", "amount", "shippingAmount", "totalAmount", "currency", "isApproximate"],
                  "properties": {
                    "rawText": { "type": "string" },
                    "amount": { "type": "number" },
                    "shippingAmount": { "type": "number" },
                    "totalAmount": { "type": "number" },
                    "currency": { "type": "string" },
                    "isApproximate": { "type": "boolean" }
                  }
                },
                "match": {
                  "type": "object",
                  "additionalProperties": false,
                  "required": ["score", "label", "matchedTerms", "missingTerms", "variantNotes"],
                  "properties": {
                    "score": { "type": "number" },
                    "label": { "type": "string", "enum": ["exact", "likely", "partial", "wrong_variant", "not_enough_data"] },
                    "matchedTerms": { "type": "array", "items": { "type": "string" } },
                    "missingTerms": { "type": "array", "items": { "type": "string" } },
                    "variantNotes": { "type": "string" }
                  }
                },
                "risk": {
                  "type": "object",
                  "additionalProperties": false,
                  "required": ["score", "level", "signals", "explanation"],
                  "properties": {
                    "score": { "type": "number" },
                    "level": { "type": "string", "enum": ["low", "medium", "high"] },
                    "signals": { "type": "array", "items": { "type": "string" } },
                    "explanation": { "type": "string" }
                  }
                }
              }
            }
          }
        }
        """;
}
