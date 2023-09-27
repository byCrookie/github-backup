using GitHubBackup.Core.Flurl;
using GitHubBackup.Core.Github;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubBackup.Core;

public static class CoreModule
{
    public static void AddCore(this IServiceCollection services)
    {
        services.AddGithub(); 
        services.AddFlurl();
    }
}