using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Github.Credentials;

internal static class CredentialModule
{
    public static void AddCredentials(this IServiceCollection services)
    {
        services.AddSingleton<IGithubTokenStore, GithubTokenStore>();
    }
}