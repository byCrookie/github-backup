using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Github.Repositories;

internal static class RepositoryModule
{
    public static void AddRepository(this IServiceCollection services)
    {
        services.AddTransient<IRepositoryService, RepositoryService>();
    }
}
