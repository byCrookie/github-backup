using System.CommandLine;
using GithubBackup.Cli.Commands.Global;

namespace GithubBackup.Cli.Commands.Github.Manual;

internal static class ManualBackupCommand
{
    private const string CommandName = "manual";
    private const string CommandDescription = "Manually backup a Github user. This command is interactive.";
    
    public static Command Create(Func<string[], GlobalArgs, ManualBackupArgs, Task> runAsync, string[] args)
    {
        var command = new Command(CommandName, CommandDescription);

        command.SetHandler(
            (globalArgs, manualBackupArgs) => runAsync(args, globalArgs, manualBackupArgs),
            new GlobalArgsBinder(),
            new ManualBackupArgsBinder()
        );

        return command;
    }
}