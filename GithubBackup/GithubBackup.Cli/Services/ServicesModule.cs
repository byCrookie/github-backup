using GithubBackup.Core.DependencyInjection.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Services;

internal static class ServicesModule
{
    /// <summary>
    /// Registers all hosted services which are executed by the application host
    /// during startup.
    /// </summary>
    /// <param name="services">Dependencies are registered on this service collection</param>
    public static void AddServices(this IServiceCollection services)
    {
        services.AddTransient<ICliCommandService, CliCommandService>();
        
        services.AddHostedService(sp => sp.GetRequiredService<IFactory<ICliCommandService>>().Create());
    }
}