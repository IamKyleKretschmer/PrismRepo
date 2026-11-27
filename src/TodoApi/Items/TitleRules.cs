using System.Globalization;
using System.Text;

namespace TodoApi.Items;

public enum TitleProblem
{
    None,
    Missing,
    Empty,
    Blank,
    TooLong,
    TooLarge,
}

/// <summary>
/// FR-2 title rules. Titles are never trimmed; length counts user-perceived characters (grapheme
/// clusters) including leading and trailing whitespace.
/// </summary>
public static class TitleRules
{
    public const int MaxCharacters = 200;

    /// <summary>Bounds size, since one grapheme cluster can hold any number of combining marks.</summary>
    public const int MaxCodePoints = 4000;

    /// <summary>The first broken rule, in order: missing, empty, too large, blank, too long.</summary>
    public static TitleProblem Check(string? title)
    {
        if (title is null)
        {
            return TitleProblem.Missing;
        }

        if (title.Length == 0)
        {
            return TitleProblem.Empty;
        }

        var codePoints = 0;
        var whitespaceOnly = true;
        foreach (var rune in title.EnumerateRunes())
        {
            if (++codePoints > MaxCodePoints)
            {
                return TitleProblem.TooLarge;
            }

            whitespaceOnly &= Rune.IsWhiteSpace(rune);
        }

        if (whitespaceOnly)
        {
            return TitleProblem.Blank;
        }

        return new StringInfo(title).LengthInTextElements > MaxCharacters ? TitleProblem.TooLong : TitleProblem.None;
    }
}
