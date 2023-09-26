using GitHubBackup.Cli.Services;
using GitHubBackup.Core;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubBackup.Cli;

internal static class CliModule
{
    public static IServiceCollection AddCli(this IServiceCollection services, Func<Task> action)
    {
        services.AddCore();
        services.AddServices(action);
        return services;
    }
}