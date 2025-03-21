using GithubBackup.Core.DependencyInjection;
using GithubBackup.Core.Environment;
using GithubBackup.Core.Github;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core;

public static class CoreModule
{
    public static void AddCore(this IServiceCollection services)
    {
        services.AddDependencyInjection();
        services.AddGithub();
        services.AddUtils();
        services.AddEnvironment();
    }
}
