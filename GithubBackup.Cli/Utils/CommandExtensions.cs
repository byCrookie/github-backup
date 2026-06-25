using System.CommandLine;

namespace GithubBackup.Cli.Utils;

public static class CommandExtensions
{
    extension(Command command)
    {
        public void AddOptions(IEnumerable<Option> options)
        {
            foreach (var option in options)
            {
                command.Add(option);
            }
        }

        public void AddCommands(IEnumerable<Command> subCommands)
        {
            foreach (var subCommand in subCommands)
            {
                command.Add(subCommand);
            }
        }
    }
}
