using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Environment;

internal static class EnvironmentModule
{
    public static void AddEnvironment(this IServiceCollection services)
    {
        services.AddSingleton<IEnvironment, Environment>();
    }
}
