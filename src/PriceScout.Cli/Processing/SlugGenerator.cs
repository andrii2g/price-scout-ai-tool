using System.Text;
using System.Text.RegularExpressions;

namespace PriceScout.Cli.Processing;

public static class SlugGenerator
{
    private static readonly Regex NonAlphaNumeric = new("[^a-z0-9]+", RegexOptions.Compiled);
    private const int MaxSlugLength = 60;

    public static string Generate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "report";
        }

        var normalized = value.Trim().ToLowerInvariant();
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            builder.Append(character is >= 'a' and <= 'z' or >= '0' and <= '9' ? character : '-');
        }

        var slug = NonAlphaNumeric.Replace(builder.ToString(), "-").Trim('-');
        if (slug.Length == 0)
        {
            slug = "report";
        }

        if (slug.Length > MaxSlugLength)
        {
            slug = slug[..MaxSlugLength].Trim('-');
        }

        return string.IsNullOrWhiteSpace(slug) ? "report" : slug;
    }
}
