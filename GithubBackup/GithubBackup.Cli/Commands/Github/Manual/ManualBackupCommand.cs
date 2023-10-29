using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Global;

namespace GithubBackup.Cli.Commands.Github.Manual;

internal static class ManualBackupCommand
{
    private const string CommandName = "manual";
    private const string CommandDescription = "Manually backup a Github user. This command is interactive.";

    public static Command Create(string[] args, CommandOptions options)
    {
        var command = new Command(CommandName, CommandDescription);

        command.SetHandler(
            (globalArgs, manualBackupArgs) => GithubBackup.Cli.Cli
                .RunAsync<ManualBackupRunner, ManualBackupArgs>(args, globalArgs, manualBackupArgs, new RunOptions
                {
                    AfterConfiguration = options.AfterConfiguration,
                    AfterServices = options.AfterServices
                }),
            new GlobalArgsBinder(options.GlobalArguments),
            new ManualBackupArgsBinder()
        );

        return command;
    }
}