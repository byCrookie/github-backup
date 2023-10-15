using System.CommandLine;
using GithubBackup.Cli.Options;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal static class MigrateCommand
{
    private const string CommandName = "migrate";
    private const string CommandDescription = "Migrate a Github user.";
    
    public static Command Create(Func<string[], GlobalArgs, MigrateArgs, Task> runAsync, string[] args)
    {
        var command = new Command(CommandName, CommandDescription);
        
        command.AddOptions(new List<Option>
        {
            MigrateArgs.RepositoriesOption,
            MigrateArgs.LockRepositoriesOption,
            MigrateArgs.ExcludeMetadataOption,
            MigrateArgs.ExcludeGitDataOption,
            MigrateArgs.ExcludeAttachementsOption,
            MigrateArgs.ExcludeReleasesOption,
            MigrateArgs.ExcludeOwnerProjectsOption,
            MigrateArgs.ExcludeMetadataOnlyOption
        });
        
        command.SetHandler(
            (globalArgs, migrateArgs) => runAsync(args, globalArgs, migrateArgs),
            new GlobalArgsBinder(),
            new MigrateArgsBinder()
        );

        return command;
    }
}