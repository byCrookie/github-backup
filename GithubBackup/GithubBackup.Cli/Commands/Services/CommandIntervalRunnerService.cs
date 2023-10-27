using Flurl.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Services;

internal sealed class CommandIntervalRunnerService : ICommandIntervalRunnerService
{
    private readonly TimeSpan _interval;
    private readonly ILogger<CommandIntervalRunnerService> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ICommandRunner _commandRunner;

    public CommandIntervalRunnerService(
        TimeSpan interval,
        ILogger<CommandIntervalRunnerService> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        ICommandRunner commandRunner)
    {
        _interval = interval;
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _commandRunner = commandRunner;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var periodicTimer = new PeriodicTimer(_interval);

        while (await periodicTimer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                _logger.LogInformation("Starting command: {Type}", _commandRunner.GetType().Name);
                await _commandRunner.RunAsync(cancellationToken);
            }
            catch (FlurlHttpException e)
            {
                var error = await e.GetResponseStringAsync();
                _logger.LogError(e, "Unhandled exception (Command: {Type}): {Message}",
                    _commandRunner.GetType().Name, error);
                await Console.Error.WriteLineAsync(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception (Command: {Type}): {Message}",
                    _commandRunner.GetType().Name, e.Message);
                await Console.Error.WriteLineAsync(e.Message);
            }
        }

        _hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}