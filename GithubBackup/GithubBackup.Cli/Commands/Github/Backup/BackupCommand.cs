using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Backup;

internal static class BackupCommand
{
    private const string CommandName = "backup";
    private const string CommandDescription = "Backup a Github user. This command is not interactive.";
    
    public static Command Create(string[] args)
    {
        var command = new Command(CommandName, CommandDescription);
        var migrateArguments = new MigrateArguments(true);
        var downloadArguments = new DownloadArguments(true);
        command.AddOptions(migrateArguments.Options());
        command.AddOptions(downloadArguments.Options());

        command.SetHandler(
            (globalArgs, manualBackupArgs) => GithubBackup.Cli.Cli.RunAsync<BackupRunner, BackupArgs>(args, globalArgs, manualBackupArgs),
            new GlobalArgsBinder(),
            new BackupArgsBinder(migrateArguments, downloadArguments)
        );

        return command;
    }
}