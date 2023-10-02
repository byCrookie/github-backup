using GithubBackup.Cli.Commands.Github.Credentials;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Commands.Github;

internal static class GithubModule
{
    public static void AddGithub(this IServiceCollection services)
    {
        services.AddTransient<IBackup, Backup>();
        
        services.AddCredentials();
    }
}