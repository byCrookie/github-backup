using GithubBackup.Core.DependencyInjection.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Commands.Services;

internal static class ServicesModule
{
    /// <summary>
    /// Registers all hosted services which are executed by the application host
    /// during startup.
    /// </summary>
    /// <param name="services">Dependencies are registered on this service collection</param>
    /// <param name="commandArgs">The arguments for the specific command</param>
    public static void AddServices<TCommandArgs>(this IServiceCollection services, TCommandArgs commandArgs)
        where TCommandArgs : class
    {
        services.AddTransient<ICommandRunnerService, CommandRunnerService>();
        services.AddTransient<ICommandIntervalRunnerService, CommandIntervalRunnerService>();

        if (commandArgs is ICommandIntervalArgs { IntervalArgs.Interval: not null } intervalArgs)
        {
            services.AddHostedService(sp => sp.GetRequiredService<IFactory<TimeSpan, ICommandIntervalRunnerService>>().Create(intervalArgs.IntervalArgs.Interval.Value));
        }
        else
        {
            services.AddHostedService(sp => sp.GetRequiredService<IFactory<ICommandRunnerService>>().Create());
        }
    }
}