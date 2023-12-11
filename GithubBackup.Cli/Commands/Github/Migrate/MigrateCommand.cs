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
    private const string CommandDescription = "Migrate a Github user. This command will create a new migration for the given repositories.";

    public static Command Create(string[] args, CommandOptions options)
    {
        var command = new Command(CommandName, CommandDescription);
        var migrateArguments = new MigrateArguments(true);
        var intervalArguments = new IntervalArguments();
        var loginArguments = new LoginArguments();
        command.AddOptions(migrateArguments.Options());
        command.AddOptions(intervalArguments.Options());
        command.AddOptions(loginArguments.Options());

        command.SetHandler(
            (globalArgs, migrateArgs) => RunAsync(args, globalArgs, migrateArgs, options),
            new GlobalArgsBinder(options.GlobalArguments),
            new MigrateArgsBinder(migrateArguments, intervalArguments, loginArguments)
        );

        return command;
    }

    private static Task RunAsync(string[] args, GlobalArgs globalArgs, MigrateArgs migrateArgs, CommandOptions options)
    {
        var runner = new CliRunner<MigrateRunner, MigrateArgs>(
            args, globalArgs, migrateArgs,
            new RunOptions
            {
                AfterServices = options.AfterServices
            });

        return runner.RunAsync();
    }
}