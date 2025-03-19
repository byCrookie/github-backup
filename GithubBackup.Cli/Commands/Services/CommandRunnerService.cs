using Flurl.Http;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Services;

internal sealed class CommandRunnerService : IHostedService
{
    private readonly GlobalArgs _globalArgs;
    private readonly ILogger<CommandRunnerService> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ICommandRunner _commandRunner;
    private readonly IAnsiConsole _ansiConsole;
    private readonly IStopwatch _stopwatch;

    public CommandRunnerService(
        GlobalArgs globalArgs,
        ILogger<CommandRunnerService> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        ICommandRunner commandRunner,
        IAnsiConsole ansiConsole,
        IStopwatch stopwatch
    )
    {
        _globalArgs = globalArgs;
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _commandRunner = commandRunner;
        _ansiConsole = ansiConsole;
        _stopwatch = stopwatch;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running command");

        if (!_globalArgs.Quiet)
        {
            _ansiConsole.WriteLine("Running command");
        }

        var stopWatch = _stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting command: {Type}", _commandRunner.GetType().Name);
            await _commandRunner.RunAsync(cancellationToken);
        }
        catch (FlurlHttpException e)
        {
            var error = await e.GetResponseStringAsync();
            _logger.LogCritical(
                e,
                "Unhandled exception (Command: {Type}): {Message}",
                _commandRunner.GetType().Name,
                error
            );
            throw new Exception(error, e);
        }
        catch (Exception e)
        {
            _logger.LogCritical(
                e,
                "Unhandled exception (Command: {Type}): {Message}",
                _commandRunner.GetType().Name,
                e.Message
            );
            throw;
        }
        finally
        {
            stopWatch.Stop();
            _logger.LogInformation("Command finished. Duration: {Duration}", stopWatch.Elapsed);

            if (!_globalArgs.Quiet)
            {
                _ansiConsole.WriteLine($"Command finished. Duration: {stopWatch.Elapsed}");
            }

            _hostApplicationLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
