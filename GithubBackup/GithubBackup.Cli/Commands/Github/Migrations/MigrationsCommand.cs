using System.CommandLine;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal static class MigrationsCommand
{
    private const string CommandName = "migrations";
    private const string CommandDescription = "List migrations.";
    
    public static Command Create(string[] args)
    {
        var command = new Command(CommandName, CommandDescription);
        var migrationsArguments = new MigrationArguments();
        command.AddOptions(migrationsArguments.Options());
        
        command.SetHandler(
            (globalArgs, migrationsArgs) => GithubBackup.Cli.Cli.RunAsync<MigrationsRunner, MigrationsArgs>(args, globalArgs, migrationsArgs),
            new GlobalArgsBinder(),
            new MigrationsArgsBinder(migrationsArguments)
        );

        return command;
    }
}