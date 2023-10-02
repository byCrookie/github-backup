using System.CommandLine;

namespace GithubBackup.Cli.Options;

internal static class OptionExtensions
{
    public static void AddOptions(this Command command, List<Option> options)
    {
        foreach (var option in options)
        {
            command.AddOption(option);
        }
    }
    
    public static void AddGlobalOptions(this Command command, List<Option> options)
    {
        foreach (var option in options)
        {
            command.AddGlobalOption(option);
        }
    }
}