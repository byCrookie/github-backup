using System.CommandLine;
using GithubBackup.Cli.Boot;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Download;

internal static class DownloadCommand
{
    private const string CommandName = "download";
    private const string CommandDescription = "Download migrations for a Github user.";

    public static Command Create(string[] args, CommandOptions options)
    {
        var command = new Command(CommandName, CommandDescription);
        var downloadArguments = new DownloadArguments(true);
        var intervalArguments = new IntervalArguments();
        var loginArguments = new LoginArguments();
        command.AddOptions(downloadArguments.Options());
        command.AddOptions(intervalArguments.Options());
        command.AddOptions(loginArguments.Options());

        command.SetHandler(
            (globalArgs, downloadArgs) => RunAsync(args, globalArgs, downloadArgs, options),
            new GlobalArgsBinder(options.GlobalArguments),
            new DowndloadArgsBinder(downloadArguments, intervalArguments, loginArguments)
        );

        return command;
    }

    private static Task RunAsync(
        string[] args,
        GlobalArgs globalArgs,
        DownloadArgs downloadArgs,
        CommandOptions options
    )
    {
        var runner = new CliRunner<DownloadRunner, DownloadArgs>(
            args,
            globalArgs,
            downloadArgs,
            new RunOptions { AfterServices = options.AfterServices }
        );

        return runner.RunAsync();
    }
}
