using System.CommandLine;

namespace GithubBackup.Cli.Utils;

public static class CommandExtensions
{
    public static void AddOptions(this Command command, IEnumerable<Option> options)
    {
        foreach (var option in options)
        {
            command.Add(option);
        }
    }

    public static void AddCommands(this Command command, IEnumerable<Command> subCommands)
    {
        foreach (var subCommand in subCommands)
        {
            command.Add(subCommand);
        }
    }
}
