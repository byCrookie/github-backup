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
    private const string CommandDescription = "List migrations for a Github user.";

    public static Command Create(string[] args, CommandOptions options)
    {
        var command = new Command(CommandName, CommandDescription);
        var migrationsArguments = new MigrationsArguments();
        var loginArguments = new LoginArguments();
        command.AddOptions(migrationsArguments.Options());
        command.AddOptions(loginArguments.Options());

        command.SetAction((r, ct) =>
        {
            var globalArgs = new GlobalArgsBinder(options.GlobalArguments).Get(r);
            var migrationsArgs = new MigrationsArgsBinder(
                migrationsArguments,
                loginArguments
            ).Get(r);

            var runner = new CliRunner<MigrationsRunner, MigrationsArgs>(
                args,
                globalArgs,
                migrationsArgs,
                new RunOptions { AfterServices = options.AfterServices }
            );

            return runner.RunAsync(ct);
        });

        return command;
    }
}
