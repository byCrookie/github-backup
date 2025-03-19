using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Console;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Boot;

internal static class Cli
{
    public static Task<int> RunAsync(string[] args, CliOptions? options = null)
    {
        var cliOptions = options ?? new CliOptions();

        var rootCommand = new RootCommand("Github Backup");

        var globalArguments = new GlobalArguments();
        rootCommand.AddGlobalOptions(globalArguments.Options());

        GithubCommands.AddCommands(
            args,
            rootCommand,
            new CommandOptions
            {
                GlobalArguments = globalArguments,
                AfterServices = cliOptions.AfterServices,
            }
        );

        return new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseExceptionHandler(
                (exception, ic) =>
                {
                    ((ICliConsole)ic.Console).WriteException(exception);
                    ic.ExitCode = 1;
                }
            )
            .Build()
            .InvokeAsync(args, cliOptions.Console);
    }
}
