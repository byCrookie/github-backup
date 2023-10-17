using System.CommandLine;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal static class RepositoriesCommand
{
    private const string CommandName = "repositories";
    private const string CommandDescription = "List repositories.";
    
    public static Command Create(string[] args)
    {
        var command = new Command(CommandName, CommandDescription);
        
        command.AddOptions(new List<Option>
        {
            RepositoriesArgs.TypeOption
        });
        
        command.SetHandler(
            (globalArgs, migrationsArgs) => GithubBackup.Cli.Cli.RunAsync<RepositoriesRunner, RepositoriesArgs>(args, globalArgs, migrationsArgs),
            new GlobalArgsBinder(),
            new RepositoriesArgsBinder()
        );

        return command;
    }
}