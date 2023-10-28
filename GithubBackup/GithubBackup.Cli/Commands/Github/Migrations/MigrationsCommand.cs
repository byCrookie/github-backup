using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal static class MigrationsCommand
{
    private const string CommandName = "migrations";
    private const string CommandDescription = "List migrations.";
    
    public static Command Create(string[] args, GlobalArguments globalArguments)
    {
        var command = new Command(CommandName, CommandDescription);
        var migrationsArguments = new MigrationsArguments();
        var loginArguments = new LoginArguments();
        command.AddOptions(migrationsArguments.Options());
        command.AddOptions(loginArguments.Options());
        
        command.SetHandler(
            (globalArgs, migrationsArgs) => GithubBackup.Cli.Cli.RunAsync<MigrationsRunner, MigrationsArgs>(args, globalArgs, migrationsArgs),
            new GlobalArgsBinder(globalArguments),
            new MigrationsArgsBinder(migrationsArguments, loginArguments)
        );

        return command;
    }
}