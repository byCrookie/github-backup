using System.CommandLine;
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
    private const string CommandDescription = "Backup a Github user.";
    
    public static Command Create(string[] args, GlobalArguments globalArguments)
    {
        var command = new Command(CommandName, CommandDescription);
        var migrateArguments = new MigrateArguments(true);
        var downloadArguments = new DownloadArguments(true);
        var intervalArguments = new IntervalArguments();
        var loginArguments = new LoginArguments();
        
        command.AddOptions(migrateArguments.Options());
        
        var downloadOptions = downloadArguments.Options();
        downloadOptions.Remove(downloadArguments.MigrationsOption);
        command.AddOptions(downloadOptions);
        
        command.AddOptions(intervalArguments.Options());
        command.AddOptions(loginArguments.Options());

        command.SetHandler(
            (globalArgs, manualBackupArgs) => GithubBackup.Cli.Cli.RunAsync<BackupRunner, BackupArgs>(args, globalArgs, manualBackupArgs),
            new GlobalArgsBinder(globalArguments),
            new BackupArgsBinder(migrateArguments, downloadArguments, intervalArguments, loginArguments)
        );

        return command;
    }
}