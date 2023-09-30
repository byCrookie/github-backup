using System.CommandLine;
using System.CommandLine.Parsing;

namespace GithubBackup.Cli.Utils;

public static class OptionExtensions
{
    public static T GetRequiredValueForOption<T>(this ParseResult result, Option<T> option)
    {
        var value = result.GetValueForOption(option);
        if (value is null)
        {
            throw new ArgumentNullException($"Option '{option.Name}' is required.");
        }

        return value;
    }
}