using System.CommandLine;
using GithubBackup.Cli.Boot;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal static class BackupCommand
{
    private const string CommandName = "backup";

    private const string CommandDescription =
        """
        Backup a Github user. This command will create a new migration
        for the given repositories and download it when ready.
        """;

    public static Command Create(string[] args, CommandOptions options)
    {
        var command = new Command(CommandName, CommandDescription);
        var migrateArguments = new MigrateArguments(true);
        var downloadArguments = new DownloadArguments(true);
        var intervalArguments = new IntervalArguments();
        var loginArguments = new LoginArguments();

        command.AddOptions(migrateArguments.Options());

        var downloadOptions = downloadArguments.Options();
        downloadOptions.Remove(downloadArguments.MigrationsOption);
        downloadOptions.Remove(downloadArguments.PollOption);
        command.AddOptions(downloadOptions);

        command.AddOptions(intervalArguments.Options());
        command.AddOptions(loginArguments.Options());

        command.SetHandler(
            (globalArgs, backupArgs) => RunAsync(args, globalArgs, backupArgs, options),
            new GlobalArgsBinder(options.GlobalArguments),
            new BackupArgsBinder(migrateArguments, downloadArguments, intervalArguments, loginArguments)
        );

        return command;
    }

    private static Task RunAsync(string[] args, GlobalArgs globalArgs, BackupArgs backupArgs, CommandOptions options)
    {
        var runner = new CliRunner<BackupRunner, BackupArgs>(
            args, globalArgs, backupArgs,
            new RunOptions
            {
                AfterServices = options.AfterServices
            });

        return runner.RunAsync();
    }
}