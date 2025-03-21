using GithubBackup.Cli.Commands.Github;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Commands.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Commands;

internal static class CommandsModule
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
    public static void AddCommands<TCliCommand, TCommandArgs>(
        this IServiceCollection services,
        GlobalArgs globalArgs,
        TCommandArgs commandArgs
    )
        where TCommandArgs : class
        where TCliCommand : class, ICommandRunner
    {
        services.AddGithub();

        services.AddTransient(s => ActivatorUtilities.CreateInstance<TCliCommand>(s, globalArgs));

        services.AddTransient<ICommandRunner>(s =>
            ActivatorUtilities.CreateInstance<TCliCommand>(s, globalArgs, commandArgs)
        );

        services.AddServices(globalArgs, commandArgs);
    }
}
