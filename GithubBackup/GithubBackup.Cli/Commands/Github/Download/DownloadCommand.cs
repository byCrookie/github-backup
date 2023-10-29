using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Download;

internal static class DownloadCommand
{
    private const string CommandName = "download";
    private const string CommandDescription = "Download migrations.";
    
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
            (globalArgs, migrateArgs) => GithubBackup.Cli.Cli
                .RunAsync<DownloadRunner, DownloadArgs>(args, globalArgs, migrateArgs, new RunOptions
                {
                    AfterConfiguration = options.AfterConfiguration,
                    AfterServices = options.AfterServices
                }),
            new GlobalArgsBinder(options.GlobalArguments),
            new DowndloadArgsBinder(downloadArguments, intervalArguments, loginArguments)
        );

        return command;
    }
}