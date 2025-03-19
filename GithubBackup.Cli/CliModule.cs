using System.IO.Abstractions;
using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;

namespace GithubBackup.Cli;

internal static class CliModule
{
    /// <summary>
    /// Registers all dependencies for the CLI. The order of registrations is important.
    /// All basic dependencies are registered first, then the cli commands.
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
        where TCliCommand : class, ICommandRunner
    {
        services.AddCore();
        services.AddSerilog();
        services.AddTransient<IFileSystem, FileSystem>();
        services.AddSingleton<IAnsiConsole>(_ => AnsiConsole.Console);

        services.AddCommands<TCliCommand, TCommandArgs>(globalArgs, commandArgs);
    }
}
