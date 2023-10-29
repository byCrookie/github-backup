using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal static class RepositoriesCommand
{
    private const string CommandName = "repositories";
    private const string CommandDescription = "List repositories.";

    public static Command Create(string[] args, CommandOptions options)
    {
        var command = new Command(CommandName, CommandDescription);
        var repositoriesArguments = new RepositoriesArguments();
        var loginArguments = new LoginArguments();
        command.AddOptions(repositoriesArguments.Options());
        command.AddOptions(loginArguments.Options());

        command.SetHandler(
            (globalArgs, migrationsArgs) => GithubBackup.Cli.Cli
                .RunAsync<RepositoriesRunner, RepositoriesArgs>(args, globalArgs, migrationsArgs, new RunOptions
                {
                    AfterConfiguration = options.AfterConfiguration,
                    AfterServices = options.AfterServices
                }),
            new GlobalArgsBinder(options.GlobalArguments),
            new RepositoriesArgsBinder(repositoriesArguments, loginArguments)
        );

        return command;
    }
}