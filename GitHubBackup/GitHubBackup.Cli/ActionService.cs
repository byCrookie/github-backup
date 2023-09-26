using Microsoft.Extensions.Hosting;

namespace GitHubBackup.Cli;

internal class ActionService : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly Func<Task> _action;

    public ActionService(IHostApplicationLifetime hostApplicationLifetime, Func<Task> action)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _action = action;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _action();
        _hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}