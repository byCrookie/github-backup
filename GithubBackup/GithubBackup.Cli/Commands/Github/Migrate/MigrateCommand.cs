using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Migrations;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal static class MigrateCommand
{
    private const string CommandName = "migrate";
    private const string CommandDescription = "Migrate a Github user.";
    
    public static Command Create(string[] args, GlobalArguments globalArguments)
    {
        var command = new Command(CommandName, CommandDescription);
        var migrateArguments = new MigrateArguments(true);
        command.AddOptions(migrateArguments.Options());
        
        command.SetHandler(
            (globalArgs, migrateArgs) => GithubBackup.Cli.Cli.RunAsync<MigrationsRunner, MigrateArgs>(args, globalArgs, migrateArgs),
            new GlobalArgsBinder(globalArguments),
            new MigrateArgsBinder(migrateArguments)
        );

        return command;
    }
}