using GithubBackup.Cli.Commands.Global;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Commands.Services;

internal static class ServicesModule
{
    public static void AddServices<TCommandArgs>(this IServiceCollection services, GlobalArgs globalArgs,
        TCommandArgs commandArgs)
        where TCommandArgs : class
    {
        if (commandArgs is ICommandIntervalArgs { IntervalArgs.Interval: not null } intervalArgs)
        {
            services
                .AddHostedService(sp => ActivatorUtilities
                    .CreateInstance<CommandIntervalRunnerService>(sp, globalArgs,
                        intervalArgs.IntervalArgs.Interval.Value));
        }
        else
        {
            services
                .AddHostedService(sp => ActivatorUtilities
                    .CreateInstance<CommandRunnerService>(sp, globalArgs));
        }
    }
}