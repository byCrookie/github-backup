using System.CommandLine;
using GithubBackup.Cli.Boot;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal static class MigrateCommand
{
    private const string CommandName = "migrate";
    private const string CommandDescription =
        "Create a GitHub migration for the selected repositories.";

    public static Command Create(string[] args, CommandOptions options)
    {
        var command = new Command(CommandName, CommandDescription);
        var migrateArguments = new MigrateArguments(true);
        var intervalArguments = new IntervalArguments();
        var loginArguments = new LoginArguments();
        command.AddOptions(migrateArguments.Options());
        command.AddOptions(intervalArguments.Options());
        command.AddOptions(loginArguments.Options());

        command.SetAction(
            (r, ct) =>
            {
                var globalArgs = new GlobalArgsBinder(options.GlobalArguments).Get(r);
                var migrateArgs = new MigrateArgsBinder(
                    migrateArguments,
                    intervalArguments,
                    loginArguments
                ).Get(r);

                var runner = new CliRunner<MigrateRunner, MigrateArgs>(
                    args,
                    globalArgs,
                    migrateArgs,
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
