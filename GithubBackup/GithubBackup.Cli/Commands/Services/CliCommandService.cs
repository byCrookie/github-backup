using Flurl.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Services;

internal sealed class CliCommandService : ICliCommandService
{
    private readonly ILogger<CliCommandService> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ICliCommand _cliCommand;

    public CliCommandService(
        ILogger<CliCommandService> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        ICliCommand cliCommand)
    {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _cliCommand = cliCommand;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Starting command: {Type}", _cliCommand.GetType().Name);
            await _cliCommand.RunAsync(cancellationToken);
        }
        catch (FlurlHttpException e)
        {
            var error = await e.GetResponseStringAsync();
            _logger.LogCritical(e, "Unhandled exception (Command: {Type}):{NewLine}{Message}",
                _cliCommand.GetType().Name, Environment.NewLine, error);
            await Console.Error.WriteLineAsync(e.Message);
            Environment.ExitCode = 1;
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unhandled exception (Command: {Type}): {Message}",
                _cliCommand.GetType().Name, e.Message);
            await Console.Error.WriteLineAsync(e.Message);
            Environment.ExitCode = 1;
        }

        _hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}