using GitHubBackup.Core.Github;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubBackup.Core;

public static class CoreModule
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddGithub();
        return services;
    }
}