using Flurl.Http;
using GithubBackup.Cli.Output;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Services;

internal sealed class CommandRunnerService(
    ILogger<CommandRunnerService> logger,
    IHostApplicationLifetime hostApplicationLifetime,
    ICommandRunner commandRunner,
    ICliOutput output,
    IStopwatch stopwatch
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running command");

        output.Status("Running command");

        var stopWatch = stopwatch.StartNew();

        try
        {
            logger.LogInformation("Starting command: {Type}", commandRunner.GetType().Name);
            await commandRunner.RunAsync(cancellationToken);
        }
        catch (FlurlHttpException e)
        {
            var error = await e.GetResponseStringAsync();
            logger.LogCritical(
                e,
                "Unhandled exception (Command: {Type}): {Message}",
                commandRunner.GetType().Name,
                error
            );
            throw new Exception(error, e);
        }
        catch (Exception e)
        {
            logger.LogCritical(
                e,
                "Unhandled exception (Command: {Type}): {Message}",
                commandRunner.GetType().Name,
                e.Message
            );
            throw;
        }
        finally
        {
            stopWatch.Stop();
            logger.LogInformation("Command finished. Duration: {Duration}", stopWatch.Elapsed);

            output.Status($"Command finished. Duration: {stopWatch.Elapsed}");

            hostApplicationLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
