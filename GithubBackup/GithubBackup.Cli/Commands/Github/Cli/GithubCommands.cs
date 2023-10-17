using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Backup;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Manual;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Github.Migrations;
using GithubBackup.Cli.Commands.Github.Repositories;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Cli;

internal static class GithubCommands
{
    public static void AddCommands(string[] args, Command command)
    {
        var manualBackupCommand = ManualBackupCommand.Create(args);
        var migrateCommand = MigrateCommand.Create(args);
        var loginCommand = LoginCommand.Create(args);
        var migrationsCommand = MigrationsCommand.Create(args);
        var repositoriesCommand = RepositoriesCommand.Create(args);
        var downloadCommand = DownloadCommand.Create(args);
        var backupCommand = BackupCommand.Create(args);

        command.AddCommands(new List<Command>
        {
            manualBackupCommand,
            migrateCommand,
            loginCommand,
            migrationsCommand,
            repositoriesCommand,
            downloadCommand,
            backupCommand
        });
    }
}