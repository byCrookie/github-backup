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
    public static void AddCommands(string[] args, Command command, GlobalArguments globalArguments)
    {
        var manualBackupCommand = ManualBackupCommand.Create(args, globalArguments);
        var migrateCommand = MigrateCommand.Create(args, globalArguments);
        var loginCommand = LoginCommand.Create(args, globalArguments);
        var migrationsCommand = MigrationsCommand.Create(args, globalArguments);
        var repositoriesCommand = RepositoriesCommand.Create(args, globalArguments);
        var downloadCommand = DownloadCommand.Create(args, globalArguments);
        var backupCommand = BackupCommand.Create(args, globalArguments);

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