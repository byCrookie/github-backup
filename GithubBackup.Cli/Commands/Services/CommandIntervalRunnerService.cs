using Flurl.Http;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Services;

internal sealed class CommandIntervalRunnerService(
    GlobalArgs globalArgs,
    TimeSpan interval,
    ILogger<CommandIntervalRunnerService> logger,
    IHostApplicationLifetime hostApplicationLifetime,
    ICommandRunner commandRunner,
    IAnsiConsole ansiConsole,
    IDateTimeProvider dateTimeProvider,
    IStopwatch stopwatch
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running command. Interval: {Interval}", interval);

        if (!globalArgs.Quiet)
        {
            ansiConsole.WriteLine($"Running command. Interval: {interval}");
        }

        var periodicTimer = new PeriodicTimer(interval);

        do
        {
            var now = dateTimeProvider.Now;
            var stopWatch = stopwatch.StartNew();

            try
            {
                logger.LogInformation("Starting command: {Type}", commandRunner.GetType().Name);
                await commandRunner.RunAsync(cancellationToken);
            }
            catch (FlurlHttpException e)
            {
                var error = await e.GetResponseStringAsync();
                logger.LogError(
                    e,
                    "Unhandled exception (Command: {Type}): {Message}",
                    commandRunner.GetType().Name,
                    error
                );
                ansiConsole.MarkupLine($"[red]{error}[/]");
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "Unhandled exception (Command: {Type}): {Message}",
                    commandRunner.GetType().Name,
                    e.Message
                );
                ansiConsole.MarkupLine($"[red]{e.Message}[/]");
            }
            finally
            {
                stopWatch.Stop();
                logger.LogInformation("Command finished. Duration: {Duration}", stopWatch.Elapsed);

                var waitUntil = now.Add(interval);
                logger.LogInformation("Waiting until {WaitUntil} for next run", waitUntil);

                if (!globalArgs.Quiet)
                {
                    ansiConsole.WriteLine($"Command finished. Duration: {stopWatch.Elapsed}");
                    ansiConsole.WriteLine($"Waiting until {waitUntil} for next run");
                }
            }
        } while (await WaitForNextTickAsync(periodicTimer, cancellationToken));

        hostApplicationLifetime.StopApplication();
    }

    private static Task<bool> WaitForNextTickAsync(
        PeriodicTimer periodicTimer,
        CancellationToken ct
    )
    {
        var wait = periodicTimer.WaitForNextTickAsync(ct);
        return wait.BoolOrCanceledAsFalseAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
