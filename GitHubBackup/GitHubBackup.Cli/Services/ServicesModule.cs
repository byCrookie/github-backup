using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GitHubBackup.Cli.Services;

internal static class ServicesModule
{
    public static IServiceCollection AddServices(this IServiceCollection services, Func<Task> action)
    {
        services.AddHostedService(sp =>
            new ActionService(sp.GetRequiredService<IHostApplicationLifetime>(), async () => await action()));
        return services;
    }
}