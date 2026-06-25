using Flurl.Http;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Services;

internal sealed class CommandRunnerService(
    GlobalArgs globalArgs,
    ILogger<CommandRunnerService> logger,
    IHostApplicationLifetime hostApplicationLifetime,
    ICommandRunner commandRunner,
    IAnsiConsole ansiConsole,
    IStopwatch stopwatch
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running command");

        if (!globalArgs.Quiet)
        {
            ansiConsole.WriteLine("Running command");
        }

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

            if (!globalArgs.Quiet)
            {
                ansiConsole.WriteLine($"Command finished. Duration: {stopWatch.Elapsed}");
            }

            hostApplicationLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
