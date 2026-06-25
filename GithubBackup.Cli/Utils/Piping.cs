using System.Text.RegularExpressions;

namespace GithubBackup.Cli.Utils;

public static partial class Piping
{
    public static Separators Separators { get; } = new([",", ":", ";", " "]);
    private static Regex ArgRegex { get; } = GetArgRegex();

    public static long[] ReadLongs(TextReader stdin, bool piping, bool force)
    {
        if (!force && (!piping || !IsPipedInput()))
        {
            return [];
        }

        var inputs = new List<long>();
        while (stdin.ReadLine() is { } line && !string.IsNullOrEmpty(line))
        {
            var ints = ArgRegex
                .Matches(line)
                .Select(m => m.Groups["match"].Value)
                .Select(long.Parse);

            inputs.AddRange(ints);
        }

        return inputs.Count == 0 ? [] : inputs.ToArray();
    }

    public static string[] ReadStrings(TextReader stdin, bool piping, bool force)
    {
        if (!force && (!piping || !IsPipedInput()))
        {
            return [];
        }

        var inputs = new List<string>();
        while (stdin.ReadLine() is { } line && !string.IsNullOrEmpty(line))
        {
            var strings = ArgRegex.Matches(line).Select(m => m.Groups["match"].Value);

            inputs.AddRange(strings);
        }

        return inputs.Count == 0 ? [] : inputs.ToArray();
    }

    private static bool IsPipedInput()
    {
        try
        {
            _ = Console.KeyAvailable;
            return false;
        }
        catch
        {
            return true;
        }
    }

    [GeneratedRegex(
        """(?<match>([\w\/-]|\.)+)|"(?<match>[\S\s]*?)"|'(?<match>[\S\s]*?)'""",
        RegexOptions.Compiled
    )]
    private static partial Regex GetArgRegex();
}
