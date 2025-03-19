using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Github.Clients;

internal static class GithubClientsModule
{
    public static void AddClients(this IServiceCollection services)
    {
        services.AddSingleton<IGithubWebClient, GithubWebClient>();
        services.AddSingleton<IGithubApiClient, GithubApiClient>();

        services.AddMemoryCache();
    }
}
