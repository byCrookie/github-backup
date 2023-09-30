using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Services;
using GithubBackup.Core;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GithubBackup.Cli;

internal static class CliModule
{
    public static void AddCli<TCliCommand>(this IServiceCollection services) where TCliCommand : class, ICliCommand
    {
        services.AddCore();
        services.AddServices<TCliCommand>();
        services.AddSerilog();
    }
}