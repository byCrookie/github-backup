using Flurl.Http;
using GithubBackup.Cli.Output;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Services;

internal sealed class CommandIntervalRunnerService(
    TimeSpan interval,
    ILogger<CommandIntervalRunnerService> logger,
    IHostApplicationLifetime hostApplicationLifetime,
    ICommandRunner commandRunner,
    ICliOutput output,
    IDateTimeProvider dateTimeProvider,
    IStopwatch stopwatch
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting interval command runner with interval {Interval}", interval);

        output.Status($"Running command every {interval}.");

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
                output.Error(error);
            }
            catch (Exception e)
            {
                logger.LogError(
                    e,
                    "Unhandled exception (Command: {Type}): {Message}",
                    commandRunner.GetType().Name,
                    e.Message
                );
                output.Error(e.Message);
            }
            finally
            {
                stopWatch.Stop();
                logger.LogInformation("Command finished in {Duration}", stopWatch.Elapsed);

                var waitUntil = now.Add(interval);
                logger.LogInformation("Next run scheduled for {WaitUntil}", waitUntil);

                output.Status($"Next run at {waitUntil}.");
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
