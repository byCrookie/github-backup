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

        command.SetAction((r, ct) =>
        {
            var globalArgs = new GlobalArgsBinder(options.GlobalArguments).Get(r);
            var repositoriesArgs = new RepositoriesArgsBinder(
                repositoriesArguments,
                loginArguments
            ).Get(r);

            var runner = new CliRunner<RepositoriesRunner, RepositoriesArgs>(
                args,
                globalArgs,
                repositoriesArgs,
                new RunOptions { AfterServices = options.AfterServices }
            );

            return runner.RunAsync(ct);
        });

        return command;
    }
}
