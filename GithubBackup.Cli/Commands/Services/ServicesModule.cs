using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.DependencyInjection.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Commands.Services;

internal static class ServicesModule
{
    public static void AddServices<TCommandArgs>(this IServiceCollection services, GlobalArgs globalArgs, TCommandArgs commandArgs)
        where TCommandArgs : class
    {
        services.AddTransient<CommandRunnerService>();
        services.AddTransient<CommandIntervalRunnerService>();

        if (commandArgs is ICommandIntervalArgs { IntervalArgs.Interval: not null } intervalArgs)
        {
            services.AddHostedService(sp => sp.GetRequiredService<IFactory<GlobalArgs, TimeSpan, CommandIntervalRunnerService>>().Create(globalArgs, intervalArgs.IntervalArgs.Interval.Value));
        }
        else
        {
            services.AddHostedService(sp => sp.GetRequiredService<IFactory<GlobalArgs, CommandRunnerService>>().Create(globalArgs));
        }
    }
}