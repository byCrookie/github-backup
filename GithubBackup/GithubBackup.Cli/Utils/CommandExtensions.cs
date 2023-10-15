using System.CommandLine;

namespace GithubBackup.Cli.Utils;

public static class CommandExtensions
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