using TodoApi.Items;

namespace TodoApi.UnitTests;

public class TitleRulesTests
{
    private const string CombiningAcute = "\u0301";

    // Family with four skin tones: 11 code points, 1 user-perceived character.
    private const string ComplexEmoji = "\U0001F468\U0001F3FB\u200D\U0001F469\U0001F3FC\u200D\U0001F467\U0001F3FD\u200D\U0001F466\U0001F3FE";

    public static TheoryData<string?, TitleProblem> Rejected() => new()
    {
        { null, TitleProblem.Missing },
        { "", TitleProblem.Empty },
        { " ", TitleProblem.Blank },
        { "     ", TitleProblem.Blank },
        { "\t\t", TitleProblem.Blank },
        { "\n\r\n", TitleProblem.Blank },
        { " \t\n ", TitleProblem.Blank },
        { "\u00A0\u2003\u3000\u2028", TitleProblem.Blank },
        { new string('a', 201), TitleProblem.TooLong },
        { " " + new string('a', 199) + " ", TitleProblem.TooLong },
        { string.Concat(Enumerable.Repeat(ComplexEmoji, 201)), TitleProblem.TooLong },
        { "a" + string.Concat(Enumerable.Repeat(CombiningAcute, 4000)), TitleProblem.TooLarge },
        { new string(' ', 4001), TitleProblem.TooLarge },
        { new string('a', 4001), TitleProblem.TooLarge },
    };

    public static TheoryData<string> Accepted() => new()
    {
        "a",
        "é",
        new string('a', 200),
        " " + new string('a', 198) + " ",
        "  leading and trailing  ",
        string.Concat(Enumerable.Repeat("e" + CombiningAcute, 100)) + string.Concat(Enumerable.Repeat("🎉", 100)),
        string.Concat(Enumerable.Repeat("é😀ñ👍🏽", 50)),
        string.Concat(Enumerable.Repeat(ComplexEmoji, 200)),
        "a" + string.Concat(Enumerable.Repeat(CombiningAcute, 3999)),
        "Купить молоко",
        "牛乳を買う",
        "\u200B",
    };

    [Theory]
    [MemberData(nameof(Rejected))]
    public void Rejects_titles_that_break_a_rule(string? title, TitleProblem expected) =>
        Assert.Equal(expected, TitleRules.Check(title));

    [Theory]
    [MemberData(nameof(Accepted))]
    public void Accepts_titles_within_every_rule(string title) =>
        Assert.Equal(TitleProblem.None, TitleRules.Check(title));

    [Fact]
    public void Complex_emoji_count_as_one_character_each()
    {
        Assert.Equal(11, ComplexEmoji.EnumerateRunes().Count());
        Assert.Equal(TitleProblem.None, TitleRules.Check(string.Concat(Enumerable.Repeat(ComplexEmoji, 200))));
        Assert.Equal(TitleProblem.TooLong, TitleRules.Check(string.Concat(Enumerable.Repeat(ComplexEmoji, 201))));
    }

    [Fact]
    public void Code_point_ceiling_is_inclusive()
    {
        var atCeiling = "a" + string.Concat(Enumerable.Repeat(CombiningAcute, TitleRules.MaxCodePoints - 1));

        Assert.Equal(TitleRules.MaxCodePoints, atCeiling.EnumerateRunes().Count());
        Assert.Equal(TitleProblem.None, TitleRules.Check(atCeiling));
        Assert.Equal(TitleProblem.TooLarge, TitleRules.Check(atCeiling + CombiningAcute));
    }

    [Fact]
    public void Too_large_takes_precedence_over_too_long() =>
        Assert.Equal(TitleProblem.TooLarge, TitleRules.Check(new string('a', TitleRules.MaxCodePoints + 1)));
}
