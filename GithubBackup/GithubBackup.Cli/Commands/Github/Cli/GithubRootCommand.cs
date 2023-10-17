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

internal static class GithubRootCommand
{
    public static RootCommand Build(string[] args)
    {
        var rootCommand = new RootCommand("Github Backup");

        rootCommand.AddGlobalOptions(new List<Option>
        {
            GlobalArgs.VerbosityOption,
            GlobalArgs.QuietOption,
            GlobalArgs.LogFileOption,
            GlobalArgs.InteractiveOption
        });

        var manualBackupCommand = ManualBackupCommand.Create(args);
        var migrateCommand = MigrateCommand.Create(args);
        var loginCommand = LoginCommand.Create(args);
        var migrationsCommand = MigrationsCommand.Create(args);
        var repositoriesCommand = RepositoriesCommand.Create(args);
        var downloadCommand = DownloadCommand.Create(args);
        var backupCommand = BackupCommand.Create(args);

        rootCommand.AddCommand(loginCommand);
        rootCommand.AddCommand(backupCommand);
        rootCommand.AddCommand(manualBackupCommand);
        rootCommand.AddCommand(migrateCommand);
        rootCommand.AddCommand(migrationsCommand);
        rootCommand.AddCommand(repositoriesCommand);
        rootCommand.AddCommand(downloadCommand);
        return rootCommand;
    }
}