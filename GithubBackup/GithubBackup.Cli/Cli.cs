using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Commands.Github.Backup;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Manual;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Github.Migrations;
using GithubBackup.Cli.Commands.Github.Repositories;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Logging;
using GithubBackup.Cli.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GithubBackup.Cli;

internal static class Cli
{
    public static async Task<int> RunAsync(string[] args)
    {
        var rootCommand = SetupCommands(args);
        var commandLineBuilder = new CommandLineBuilder(rootCommand);
        commandLineBuilder.UseDefaults();
        commandLineBuilder.UseExceptionHandler((exception, _) =>
        {
            Console.Error.WriteLine(exception.Message);
            Environment.ExitCode = 1;
        });
        var parser = commandLineBuilder.Build();
        await parser.InvokeAsync(args);
        return Environment.ExitCode;
    }

    private static RootCommand SetupCommands(string[] args)
    {
        var rootCommand = new RootCommand("Github Backup");

        rootCommand.AddGlobalOptions(new List<Option>
        {
            GlobalArgs.VerbosityOption,
            GlobalArgs.QuietOption,
            GlobalArgs.LogFileOption,
            GlobalArgs.InteractiveOption
        });

        var manualBackupCommand = ManualBackupCommand.Create(
            (_, globalArgs, manualBackupArgs) => RunAsync<ManualBackup, ManualBackupArgs>(args, globalArgs, manualBackupArgs),
            args
        );

        var migrateCommand = MigrateCommand.Create(
            (_, globalArgs, migrateArgs) => RunAsync<Migrate, MigrateArgs>(args, globalArgs, migrateArgs),
            args
        );

        var loginCommand = LoginCommand.Create(
            (_, globalArgs, loginArgs) => RunAsync<Login, LoginArgs>(args, globalArgs, loginArgs),
            args
        );

        var migrationsCommand = MigrationsCommand.Create(
            (_, globalArgs, migrationsArgs) => RunAsync<Migrations, MigrationsArgs>(args, globalArgs, migrationsArgs),
            args
        );

        var repositoriesCommand = RepositoriesCommand.Create(
            (_, globalArgs, migrationsArgs) => RunAsync<Repositories, RepositoriesArgs>(args, globalArgs, migrationsArgs),
            args
        );

        var downloadCommand = DownloadCommand.Create(
            (_, globalArgs, migrationsArgs) => RunAsync<Download, DownloadArgs>(args, globalArgs, migrationsArgs),
            args
        );

        var backupCommand = BackupCommand.Create(
            (_, globalArgs, backupArgs) => RunAsync<Backup, BackupArgs>(args, globalArgs, backupArgs),
            args
        );

        rootCommand.AddCommand(loginCommand);
        rootCommand.AddCommand(backupCommand);
        rootCommand.AddCommand(manualBackupCommand);
        rootCommand.AddCommand(migrateCommand);
        rootCommand.AddCommand(migrationsCommand);
        rootCommand.AddCommand(repositoriesCommand);
        rootCommand.AddCommand(downloadCommand);
        return rootCommand;
    }

    /// <summary>Configures the host and runs the CLI.</summary>
    /// <param name="args">The original arguments passed to the application.</param>
    /// <param name="globalArgs">The global CLI arguments.</param>
    /// <param name="commandArgs">The command-specific CLI arguments.</param>
    /// <typeparam name="TCliCommand">The implementation type of the CLI command.</typeparam>
    /// <typeparam name="TCommandArgs">The type of the CLI command arguments.</typeparam>
    /// <returns>The task that will complete when the CLI is done.</returns>
    private static Task RunAsync<TCliCommand, TCommandArgs>(string[] args, GlobalArgs globalArgs, TCommandArgs commandArgs)
        where TCommandArgs : class
        where TCliCommand : class, ICliCommand
    {
        Log.Logger = CliLoggerConfiguration
            .Create(globalArgs)
            .CreateLogger();

        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Configuration.AddEnvironmentVariables("GITHUB_BACKUP_");
        
        builder.Services.AddCli<TCliCommand, TCommandArgs>(globalArgs, commandArgs);

        var host = builder.Build();
        return host.RunAsync();
    }
}