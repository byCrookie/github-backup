using System.CommandLine;
using GithubBackup.Cli.Boot;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Repositories;

internal static class RepositoriesCommand
{
    private const string CommandName = "repositories";
    private const string CommandDescription = "List repositories for a Github user.";

    public static Command Create(string[] args, CommandOptions options)
    {
        var command = new Command(CommandName, CommandDescription);
        var repositoriesArguments = new RepositoriesArguments();
        var loginArguments = new LoginArguments();
        command.AddOptions(repositoriesArguments.Options());
        command.AddOptions(loginArguments.Options());

        command.SetHandler(
            (globalArgs, repositoriesArgs) => RunAsync(args, globalArgs, repositoriesArgs, options),
            new GlobalArgsBinder(options.GlobalArguments),
            new RepositoriesArgsBinder(repositoriesArguments, loginArguments)
        );

        return command;
    }

    private static Task RunAsync(
        string[] args,
        GlobalArgs globalArgs,
        RepositoriesArgs repositoriesArgs,
        CommandOptions options
    )
    {
        var runner = new CliRunner<RepositoriesRunner, RepositoriesArgs>(
            args,
            globalArgs,
            repositoriesArgs,
            new RunOptions { AfterServices = options.AfterServices }
        );

        return runner.RunAsync();
    }
}
