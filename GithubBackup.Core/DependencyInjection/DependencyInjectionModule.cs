using GithubBackup.Core.DependencyInjection.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.DependencyInjection;

internal static class DependencyInjectionModule
{
    public static void AddDependencyInjection(this IServiceCollection services)
    {
        services.AddFactory();
    }
}
