using System.CommandLine;
using GithubBackup.Cli.Options;

namespace GithubBackup.Cli.Commands.Github.Manual;

internal static class ManualBackupCommand
{
    private const string CommandName = "manual";
    private const string CommandDescription = "Manually backup a Github user. Interactive is always enabled for this command.";
    
    public static Command Create(Func<string[], GlobalArgs, ManualBackupArgs, Task> runAsync, string[] args)
    {
        var manualBackupCommand = new Command(CommandName, CommandDescription);

        manualBackupCommand.SetHandler(
            (globalArgs, manualBackupArgs) => runAsync(args, globalArgs, manualBackupArgs),
            new GlobalArgsBinder(),
            new ManualBackupArgsBinder()
        );

        return manualBackupCommand;
    }
}