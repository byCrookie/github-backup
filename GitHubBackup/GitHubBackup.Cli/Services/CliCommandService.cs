using Flurl.Http;
using GithubBackup.Cli.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Services;

internal class CliCommandService : IHostedService
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
            await _cliCommand.RunAsync(cancellationToken);
        }
        catch (FlurlHttpException e)
        {
            var error = await e.GetResponseStringAsync();
            _logger.LogCritical(e, "Unhandled exception (Command: {Type}):{NewLine}{Message}",
                _cliCommand.GetType().Name, Environment.NewLine, error);
            Environment.ExitCode = 1;
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unhandled exception (Command: {Type}): {Message}",
                _cliCommand.GetType().Name, e.Message);
            Environment.ExitCode = 1;
        }

        _hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}