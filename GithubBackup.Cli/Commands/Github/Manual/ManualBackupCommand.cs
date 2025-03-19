using System.CommandLine;
using GithubBackup.Cli.Boot;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Global;

namespace GithubBackup.Cli.Commands.Github.Manual;

internal static class ManualBackupCommand
{
    private const string CommandName = "manual";
    private const string CommandDescription =
        "Manually backup a Github user. This command is interactive.";

    public static Command Create(string[] args, CommandOptions options)
    {
        var command = new Command(CommandName, CommandDescription);

        command.SetHandler(
            (globalArgs, manualBackupArgs) => RunAsync(args, globalArgs, manualBackupArgs, options),
            new GlobalArgsBinder(options.GlobalArguments),
            new ManualBackupArgsBinder()
        );

        return command;
    }

    private static Task RunAsync(
        string[] args,
        GlobalArgs globalArgs,
        ManualBackupArgs manualBackupArgs,
        CommandOptions options
    )
    {
        var runner = new CliRunner<ManualBackupRunner, ManualBackupArgs>(
            args,
            globalArgs,
            manualBackupArgs,
            new RunOptions { AfterServices = options.AfterServices }
        );

        return runner.RunAsync();
    }
}
