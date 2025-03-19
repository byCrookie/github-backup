using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Backup;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Manual;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Github.Migrations;
using GithubBackup.Cli.Commands.Github.Repositories;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Cli;

internal static class GithubCommands
{
    public static void AddCommands(string[] args, Command command, CommandOptions commandOptions)
    {
        var manualBackupCommand = ManualBackupCommand.Create(args, commandOptions);
        var migrateCommand = MigrateCommand.Create(args, commandOptions);
        var loginCommand = LoginCommand.Create(args, commandOptions);
        var migrationsCommand = MigrationsCommand.Create(args, commandOptions);
        var repositoriesCommand = RepositoriesCommand.Create(args, commandOptions);
        var downloadCommand = DownloadCommand.Create(args, commandOptions);
        var backupCommand = BackupCommand.Create(args, commandOptions);

        command.AddCommands(
            new List<Command>
            {
                manualBackupCommand,
                migrateCommand,
                loginCommand,
                migrationsCommand,
                repositoriesCommand,
                downloadCommand,
                backupCommand,
            }
        );
    }
}
