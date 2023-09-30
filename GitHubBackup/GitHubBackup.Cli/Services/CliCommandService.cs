using GithubBackup.Cli.Commands;
using Microsoft.Extensions.Hosting;

namespace GithubBackup.Cli.Services;

internal class CliCommandService : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ICliCommand _cliCommand;

    public CliCommandService(IHostApplicationLifetime hostApplicationLifetime, ICliCommand cliCommand)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _cliCommand = cliCommand;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _cliCommand.RunAsync(cancellationToken);
        _hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}