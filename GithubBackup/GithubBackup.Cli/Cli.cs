using System.CommandLine;
using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Commands.Github.Backup;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Logging;
using GithubBackup.Cli.Options;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GithubBackup.Cli;

internal static class Cli
{
    public static async Task<int> RunAsync(string[] args)
    {
        var rootCommand = new RootCommand("Github Backup");

        rootCommand.AddGlobalOptions(new List<Option>
        {
            GlobalArgs.VerbosityOption,
            GlobalArgs.QuietOption,
            GlobalArgs.LogFileOption,
            GlobalArgs.InteractiveOption
        });
        
        var backupCommand = new Command("backup", "Backup a Github user. Interactive is always enabled for this command.");

        backupCommand.SetHandler(
            (globalArgs, backupArgs) => RunAsync<Backup, GithubBackupArgs>(args, globalArgs, backupArgs),
            new GlobalArgsBinder(),
            new GithubBackupArgsBinder()
        );
        
        var migrateCommand = new Command("migrate", "Start a Github user migration.");
        
        migrateCommand.AddOptions(new List<Option>
        {
            MigrateArgs.RepositoriesOption,
            MigrateArgs.LockRepositoriesOption,
            MigrateArgs.ExcludeMetadataOption,
            MigrateArgs.ExcludeGitDataOption,
            MigrateArgs.ExcludeAttachementsOption,
            MigrateArgs.ExcludeReleasesOption,
            MigrateArgs.ExcludeOwnerProjectsOption,
            MigrateArgs.ExcludeMetadataOnlyOption
        });
        
        migrateCommand.SetHandler(
            (globalArgs, migrateArgs) => RunAsync<Migrate, MigrateArgs>(args, globalArgs, migrateArgs),
            new GlobalArgsBinder(),
            new MigrateArgsBinder()
        );

        rootCommand.AddCommand(backupCommand);
        rootCommand.AddCommand(migrateCommand);
        
        await rootCommand.InvokeAsync(args);
        return Environment.ExitCode;
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
        builder.Services.AddCli<TCliCommand, TCommandArgs>(globalArgs, commandArgs);

        var host = builder.Build();
        return host.RunAsync();
    }
}