using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Github.GithubCredentials;

internal static class CredentialsModule
{
    public static void AddCredentials(this IServiceCollection services)
    {
        services.AddTransient<IAppSettingsCredentialsStore, AppSettingsCredentialsesStore>();
    }
}