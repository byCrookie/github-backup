using GitHubBackup.Cli.Commands;
using GitHubBackup.Cli.Services;
using GitHubBackup.Core;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GitHubBackup.Cli;

internal static class CliModule
{
    public static void AddCli<TCliCommand>(this IServiceCollection services) where TCliCommand : class, ICliCommand
    {
        services.AddCore();
        services.AddServices<TCliCommand>();
        services.AddSerilog();
    }
}