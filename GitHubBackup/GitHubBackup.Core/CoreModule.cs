using GithubBackup.Core.Flurl;
using GithubBackup.Core.Github;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core;

public static class CoreModule
{
    public static void AddCore(this IServiceCollection services)
    {
        services.AddGithub();
        services.AddFlurl();
    }
}