using Flurl.Http;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Services;

internal sealed class CommandIntervalRunnerService : ICommandIntervalRunnerService
{
    private readonly GlobalArgs _globalArgs;
    private readonly TimeSpan _interval;
    private readonly ILogger<CommandIntervalRunnerService> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ICommandRunner _commandRunner;
    private readonly IAnsiConsole _ansiConsole;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IStopwatch _stopwatch;

    public CommandIntervalRunnerService(
        GlobalArgs globalArgs,
        TimeSpan interval,
        ILogger<CommandIntervalRunnerService> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        ICommandRunner commandRunner,
        IAnsiConsole ansiConsole,
        IDateTimeProvider dateTimeProvider,
        IStopwatch stopwatch)
    {
        _globalArgs = globalArgs;
        _interval = interval;
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _commandRunner = commandRunner;
        _ansiConsole = ansiConsole;
        _dateTimeProvider = dateTimeProvider;
        _stopwatch = stopwatch;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running command. Interval: {Interval}", _interval);

        if (!_globalArgs.Quiet)
        {
            _ansiConsole.WriteLine($"Running command. Interval: {_interval}");
        }
        
        var periodicTimer = new PeriodicTimer(_interval);

        while (await periodicTimer.WaitForNextTickAsync(cancellationToken))
        {
            var now = _dateTimeProvider.Now;
            var stopWatch = _stopwatch.StartNew();
            
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
            finally
            {
                stopWatch.Stop();
                _logger.LogInformation("Command finished. Duration: {Duration}", stopWatch.Elapsed);
                
                var waitUntil = now.Add(_interval);
                _logger.LogInformation("Waiting until {WaitUntil} for next run", waitUntil);

                if (!_globalArgs.Quiet)
                {
                    _ansiConsole.WriteLine($"Command finished. Duration: {stopWatch.Elapsed}");
                    _ansiConsole.WriteLine($"Waiting until {waitUntil} for next run");
                }
            }
        }

        _hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}