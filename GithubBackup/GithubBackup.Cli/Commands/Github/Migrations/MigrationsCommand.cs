using System.CommandLine;
using GithubBackup.Cli.Options;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal static class MigrationsCommand
{
    private const string CommandName = "migrations";
    private const string CommandDescription = "List migrations.";
    
    public static Command Create(Func<string[], GlobalArgs, MigrationsArgs, Task> runAsync, string[] args)
    {
        var command = new Command(CommandName, CommandDescription);
        
        command.AddOptions(new List<Option>
        {
            MigrationsArgs.IdOption
        });
        
        command.SetHandler(
            (globalArgs, migrationsArgs) => runAsync(args, globalArgs, migrationsArgs),
            new GlobalArgsBinder(),
            new MigrationsArgsBinder()
        );

        return command;
    }
}