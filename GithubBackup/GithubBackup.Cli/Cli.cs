using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Commands.Github.Cli;
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
        var rootCommand = new RootCommand("Github Backup");
        var globalArguments = new GlobalArguments();
        rootCommand.AddGlobalOptions(globalArguments.Options());
        GithubCommands.AddCommands(args, rootCommand, globalArguments);

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

    /// <summary>Configures the host and runs the CLI.</summary>
    /// <param name="args">The original arguments passed to the application.</param>
    /// <param name="globalArgs">The global CLI arguments.</param>
    /// <param name="commandArgs">The command-specific CLI arguments.</param>
    /// <typeparam name="TCliCommand">The implementation type of the CLI command.</typeparam>
    /// <typeparam name="TCommandArgs">The type of the CLI command arguments.</typeparam>
    /// <returns>The task that will complete when the CLI is done.</returns>
    public static Task RunAsync<TCliCommand, TCommandArgs>(string[] args, GlobalArgs globalArgs,
        TCommandArgs commandArgs)
        where TCommandArgs : class
        where TCliCommand : class, ICommandRunner
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