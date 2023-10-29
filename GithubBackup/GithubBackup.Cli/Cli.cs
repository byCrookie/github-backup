using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Commands.Github.Cli;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Console;
using GithubBackup.Cli.Logging;
using GithubBackup.Cli.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GithubBackup.Cli;

internal static class Cli
{
    public static Task<int> RunAsync(string[] args, CliOptions? options = null)
    {
        var cliOptions = options ?? new CliOptions();

        var rootCommand = new RootCommand("Github Backup");

        var globalArguments = new GlobalArguments();
        rootCommand.AddGlobalOptions(globalArguments.Options());

        GithubCommands.AddCommands(
            args,
            rootCommand,
            new CommandOptions
            {
                GlobalArguments = globalArguments,
                AfterConfiguration = cliOptions.AfterConfiguration,
                AfterServices = cliOptions.AfterServices
            }
        );

        return new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseExceptionHandler((exception, ic) =>
            {
                ((ICliConsole)ic.Console).WriteException(exception);
                ic.ExitCode = 1;
            })
            .Build()
            .InvokeAsync(args, cliOptions.Console);
    }

    public static Task RunAsync<TCliCommand, TCommandArgs>(
        string[] args,
        GlobalArgs globalArgs,
        TCommandArgs commandArgs,
        RunOptions options
    )
        where TCommandArgs : class
        where TCliCommand : class, ICommandRunner
    {
        Log.Logger = CliLoggerConfiguration
            .Create(globalArgs)
            .CreateLogger();

        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.AddEnvironmentVariables("GITHUB_BACKUP_");
        options.AfterConfiguration.Invoke(builder);

        builder.Services.AddCli<TCliCommand, TCommandArgs>(globalArgs, commandArgs);
        options.AfterServices.Invoke(builder);

        var host = builder.Build();
        return host.RunAsync();
    }
}