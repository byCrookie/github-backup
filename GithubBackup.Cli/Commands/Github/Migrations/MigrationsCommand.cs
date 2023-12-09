using System.CommandLine;
using GithubBackup.Cli.Boot;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal static class MigrationsCommand
{
    private const string CommandName = "migrations";
    private const string CommandDescription = "List migrations.";

    public static Command Create(string[] args, CommandOptions options)
    {
        var command = new Command(CommandName, CommandDescription);
        var migrationsArguments = new MigrationsArguments();
        var loginArguments = new LoginArguments();
        command.AddOptions(migrationsArguments.Options());
        command.AddOptions(loginArguments.Options());

        command.SetHandler(
            (globalArgs, migrationsArgs) => RunAsync(args, globalArgs, migrationsArgs, options),
            new GlobalArgsBinder(options.GlobalArguments),
            new MigrationsArgsBinder(migrationsArguments, loginArguments)
        );

        return command;
    }

    private static Task RunAsync(string[] args, GlobalArgs globalArgs, MigrationsArgs migrationsArgs,
        CommandOptions options)
    {
        var runner = new CliRunner<MigrationsRunner, MigrationsArgs>(
            args, globalArgs, migrationsArgs,
            new RunOptions
            {
                AfterServices = options.AfterServices
            });

        return runner.RunAsync();
    }
}