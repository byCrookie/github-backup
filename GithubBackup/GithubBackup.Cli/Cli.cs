﻿using System.CommandLine;
using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Commands.Github;
using GithubBackup.Cli.Commands.Services;
using GithubBackup.Cli.Logging;
using GithubBackup.Cli.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using StrongInject;
using StrongInject.Extensions.DependencyInjection;

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

        rootCommand.AddOptions(new List<Option>
        {
            GithubBackupArgs.DestinationOption
        });

        rootCommand.SetHandler(
            (globalArgs, backupArgs) => RunAsync<Backup, GithubBackupArgs>(args, globalArgs, backupArgs),
            new GlobalArgsBinder(),
            new GithubBackupArgsBinder()
        );

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
    private static Task RunAsync<TCliCommand, TCommandArgs>(string[] args, GlobalArgs globalArgs,
        TCommandArgs commandArgs)
        where TCommandArgs : class
        where TCliCommand : class, ICliCommand
    {
        Log.Logger = CliLoggerConfiguration
            .Create(globalArgs)
            .CreateLogger();

        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSerilog();
        builder.Services.AddTransient<IContainer<CliCommandService>>(s =>
            new Container<TCliCommand, TCommandArgs>(s, globalArgs, commandArgs));
        builder.Services.AddTransientServiceUsingContainer<IContainer<Backup>, Backup>();
        builder.Services.AddTransientServiceUsingScopedContainer<IContainer<CliCommandService>, CliCommandService>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<CliCommandService>());

        var host = builder.Build();
        Core.Core.Initialize();
        return host.RunAsync();
    }
}