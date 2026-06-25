using System.CommandLine;
using GithubBackup.Cli.Boot;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Global;

namespace GithubBackup.Cli.Commands.Github.Manual;

internal static class ManualBackupCommand
{
    private const string CommandName = "manual";
    private const string CommandDescription =
        "Interactively create and download GitHub backups.";

    public static Command Create(string[] args, CommandOptions options)
    {
        var command = new Command(CommandName, CommandDescription);

        command.SetAction(
            (r, ct) =>
            {
                var globalArgs = new GlobalArgsBinder(options.GlobalArguments).Get(r);
                var manualBackupArgs = new ManualBackupArgsBinder().Get(r);

                var runner = new CliRunner<ManualBackupRunner, ManualBackupArgs>(
                    args,
                    globalArgs,
                    manualBackupArgs,
                    new RunOptions
                    {
                        Output = options.Output,
                        Error = options.Error,
                        AfterServices = options.AfterServices,
                    }
                );

                return runner.RunAsync(ct);
            }
        );

        return command;
    }
}
