using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Boot;

internal static class Cli
{
    public static Task<int> RunAsync(string[] args, CliOptions? options = null)
    {
        var cliOptions = options ?? new CliOptions();

        var rootCommand = new RootCommand("Github Backup");

        var globalArguments = new GlobalArguments();
        rootCommand.AddOptions(globalArguments.Options());

        GithubCommands.AddCommands(
            args,
            rootCommand,
            new CommandOptions
            {
                GlobalArguments = globalArguments,
                AfterServices = cliOptions.AfterServices,
            }
        );

        return rootCommand.Parse(args).InvokeAsync(new InvocationConfiguration
        {
            EnableDefaultExceptionHandler = cliOptions.EnableDefaultExceptionHandler,
            Output = cliOptions.Output,
            Error = cliOptions.Error
        });
    }
}
