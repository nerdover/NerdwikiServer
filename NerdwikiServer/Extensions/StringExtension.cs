using System.Text.RegularExpressions;

namespace NerdwikiServer.Extensions;

public static partial class StringExtension
{
    public static string NormalizedId(this string id)
    {
        return NormalizedName(id).Replace(" ", "-").ToLower();
    }

    public static string NormalizedName(this string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        return NotLetterNumberSpace().Replace(MoreThanOneSpace().Replace(name, " "), "").Trim();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MoreThanOneSpace();

    [GeneratedRegex(@"[^A-Za-z0-9\u0E00-\u0E7F- ]")]
    private static partial Regex NotLetterNumberSpace();
}
