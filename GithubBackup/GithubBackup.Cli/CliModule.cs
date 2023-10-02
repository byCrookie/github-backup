using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Github;
using GithubBackup.Cli.Options;
using GithubBackup.Cli.Services;
using GithubBackup.Core;
using GithubBackup.Core.DependencyInjection.Factory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GithubBackup.Cli;

internal static class CliModule
{
    /// <summary>
    /// Registers all dependencies for the CLI. The order of registrations is important.
    /// All basic dependencies are registered first, then the cli commands and then the services
    /// who execute commands.
    /// </summary>
    /// <param name="services">Dependencies are registered on this service collection.</param>
    /// <param name="globalArgs">Global application arguments valid for all commands.</param>
    /// <param name="commandArgs">Command arguments only valid for this command.</param>
    /// <typeparam name="TCliCommand">Type of the cli command's implementation.</typeparam>
    /// <typeparam name="TCommandArgs">Type of the cli command's arguments.</typeparam>
    public static void AddCli<TCliCommand, TCommandArgs>(
        this IServiceCollection services,
        GlobalArgs globalArgs,
        TCommandArgs commandArgs
    )
        where TCommandArgs : class
        where TCliCommand : class, ICliCommand
    {
        services.AddCore();
        services.AddSerilog();
        services.AddGithub();

        services.AddTransient<ICliCommand>(s => s
            .GetRequiredService<IFactory<GlobalArgs, TCommandArgs, TCliCommand>>()
            .Create(globalArgs, commandArgs));

        services.AddServices();
    }
}