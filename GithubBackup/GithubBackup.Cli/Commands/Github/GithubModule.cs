using GithubBackup.Cli.Commands.Github.Backup;
using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Github.Migrate;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Commands.Github;

internal static class GithubModule
{
    public static void AddGithub(this IServiceCollection services)
    {
        services.AddTransient<IBackup, Backup.Backup>();
        services.AddTransient<IMigrate, Migrate.Migrate>();
        
        services.AddCredentials();
    }
}