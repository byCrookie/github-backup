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
        logger.LogInformation("Running command. Interval: {Interval}", interval);

        output.Status($"Running command. Interval: {interval}");

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
                logger.LogInformation("Command finished. Duration: {Duration}", stopWatch.Elapsed);

                var waitUntil = now.Add(interval);
                logger.LogInformation("Waiting until {WaitUntil} for next run", waitUntil);

                output.Status($"Command finished. Duration: {stopWatch.Elapsed}");
                output.Status($"Waiting until {waitUntil} for next run");
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
