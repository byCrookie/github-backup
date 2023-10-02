using GithubBackup.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Services;

internal static class ServicesModule
{
    public static void AddServices<TCliCommand>(this IServiceCollection services)
        where TCliCommand : class, ICliCommand
    {
        services.AddHostedService(sp => new CliCommandService(
            sp.GetRequiredService<ILogger<CliCommandService>>(),
            sp.GetRequiredService<IHostApplicationLifetime>(),
            sp.GetRequiredService<TCliCommand>()
        ));
    }
}