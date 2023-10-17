using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Migrations;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal static class MigrateCommand
{
    private const string CommandName = "migrate";
    private const string CommandDescription = "Migrate a Github user.";
    
    public static Command Create(string[] args)
    {
        var command = new Command(CommandName, CommandDescription);
        command.AddOptions(MigrateArgs.Options());
        
        command.SetHandler(
            (globalArgs, migrateArgs) => GithubBackup.Cli.Cli.RunAsync<MigrationsRunner, MigrateArgs>(args, globalArgs, migrateArgs),
            new GlobalArgsBinder(),
            new MigrateArgsBinder()
        );

        return command;
    }
}