using GithubBackup.Cli.Github.GithubCredentials;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Github;

internal static class GithubModule
{
    public static void AddGithub(this IServiceCollection services)
    {
        services.AddCredentials();
    }
}