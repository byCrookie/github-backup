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

        command.SetAction((r, ct) =>
        {
            var globalArgs = new GlobalArgsBinder(options.GlobalArguments).Get(r);
            var downloadArgs = new DowndloadArgsBinder(
                downloadArguments,
                intervalArguments,
                loginArguments
            ).Get(r);

            var runner = new CliRunner<DownloadRunner, DownloadArgs>(
                args,
                globalArgs,
                downloadArgs,
                new RunOptions { AfterServices = options.AfterServices }
            );

            return runner.RunAsync(ct);
        });

        return command;
    }
}
