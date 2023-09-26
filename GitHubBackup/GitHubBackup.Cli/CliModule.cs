using GitHubBackup.Cli.Github;
using GitHubBackup.Cli.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubBackup.Cli;

internal static class CliModule
{
    public static IServiceCollection AddCli(this IServiceCollection services, Func<Task> action)
    {
        services.AddGithub();
        services.AddServices(action);
        return services;
    }
}