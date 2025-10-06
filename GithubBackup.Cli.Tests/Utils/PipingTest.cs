using AwesomeAssertions;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Tests.Utils;

public class PipingTests
{
    [Fact]
    public void ReadLongs_SingleDigits_Parse()
    {
        const string input = "1,2:3;4 5 6 7  8   9";
        var expected = new[] { 1L, 2L, 3L, 4L, 5L, 6L, 7L, 8L, 9L };
        var textReader = new StringReader(input);

        var result = Piping.ReadLongs(textReader, true, true);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ReadStrings_SingleCharacters_Parse()
    {
        const string input = "a,b:c;d e f g  h   i";
        var expected = new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i" };
        var textReader = new StringReader(input);

        var result = Piping.ReadStrings(textReader, true, true);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ReadLongs_MultipleDigits_Parse()
    {
        const string input = "12,23:34;45 5667 78  89   910";
        var expected = new[] { 12L, 23L, 34L, 45L, 5667L, 78L, 89L, 910L };
        var textReader = new StringReader(input);

        var result = Piping.ReadLongs(textReader, true, true);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ReadStrings_MultipleCharacters_Parse()
    {
        const string input = "ab,bc:cd;de ef-fg gh  hi   ij";
        var expected = new[] { "ab", "bc", "cd", "de", "ef-fg", "gh", "hi", "ij" };
        var textReader = new StringReader(input);

        var result = Piping.ReadStrings(textReader, true, true);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ReadStrings_MixedQuotes_Parse()
    {
        const string input = "ab,b/c.d:c-_d;de|e\"Quote,:; d1\"f-'fg' gh  \"hi\"   ij";
        var expected = new[]
        {
            "ab",
            "b/c.d",
            "c-_d",
            "de",
            "e",
            "Quote,:; d1",
            "f-",
            "fg",
            "gh",
            "hi",
            "ij",
        };
        var textReader = new StringReader(input);

        var result = Piping.ReadStrings(textReader, true, true);

        result.Should().BeEquivalentTo(expected);
    }
}
