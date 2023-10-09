using System.CommandLine;
using GithubBackup.Cli.Options;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal static class MigrationsCommand
{
    private const string CommandName = "migrations";
    private const string CommandDescription = "List migrations. Output can be piped to 'download' for further processing.";
    
    public static Command Create(Func<string[], GlobalArgs, MigrationsArgs, Task> runAsync, string[] args)
    {
        var migrateCommand = new Command(CommandName, CommandDescription);
        
        migrateCommand.AddOptions(new List<Option>
        {
            MigrationsArgs.IdOption
        });
        
        migrateCommand.SetHandler(
            (globalArgs, migrationsArgs) => runAsync(args, globalArgs, migrationsArgs),
            new GlobalArgsBinder(),
            new MigrationsArgsBinder()
        );

        return migrateCommand;
    }
}