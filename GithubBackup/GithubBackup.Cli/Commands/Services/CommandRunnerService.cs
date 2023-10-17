using Flurl.Http;
using GithubBackup.Core.Environment;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Environment = System.Environment;

namespace GithubBackup.Cli.Commands.Services;

internal sealed class CommandRunnerService : ICommandRunnerService
{
    private readonly ILogger<CommandRunnerService> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ICommandRunner _commandRunner;
    private readonly IEnvironment _environment;

    public CommandRunnerService(
        ILogger<CommandRunnerService> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        ICommandRunner commandRunner,
        IEnvironment environment)
    {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _commandRunner = commandRunner;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting command: {Type}", _commandRunner.GetType().Name);
            await _commandRunner.RunAsync(cancellationToken);
        }
        catch (FlurlHttpException e)
        {
            var error = await e.GetResponseStringAsync();
            _logger.LogCritical(e, "Unhandled exception (Command: {Type}): {Message}",
                _commandRunner.GetType().Name, error);
            await Console.Error.WriteLineAsync(e.Message);
            _environment.ExitCode = 1;
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unhandled exception (Command: {Type}): {Message}",
                _commandRunner.GetType().Name, e.Message);
            await Console.Error.WriteLineAsync(e.Message);
            _environment.ExitCode = 1;
        }

        _hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}