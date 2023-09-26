using Microsoft.Extensions.DependencyInjection;

namespace GitHubBackup.Cli.Github;

internal static class GithubModule
{
    public static IServiceCollection AddGithub(this IServiceCollection services)
    {
        services.AddTransient<IGithubBackup, GithubBackup>();
        return services;
    }
}