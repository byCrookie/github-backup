using Flurl.Http;
using GithubBackup.Cli.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Cli.Commands.Services;

internal class CliCommandService<TCliCommand, TCliArguments> : IHostedService
    where TCliCommand : class, ICliCommand
    where TCliArguments : class
{
    private readonly ILogger<CliCommandService<TCliCommand, TCliArguments>> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<GlobalArgs, TCliArguments, TCliCommand> _cliCommandFactory;

    public CliCommandService(
        ILogger<CliCommandService<TCliCommand, TCliArguments>> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        IServiceProvider serviceProvider,
        Func<GlobalArgs, TCliArguments, TCliCommand> cliCommandFactory)
    {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _serviceProvider = serviceProvider;
        _cliCommandFactory = cliCommandFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var globalArgs = _serviceProvider.GetRequiredService<GlobalArgs>();
            var commandArgs = _serviceProvider.GetRequiredService<TCliArguments>();
            await _cliCommandFactory(globalArgs, commandArgs).RunAsync(cancellationToken);
        }
        catch (FlurlHttpException e)
        {
            var error = await e.GetResponseStringAsync();
            _logger.LogCritical(e, "Unhandled exception (Command: {Type}):{NewLine}{Message}",
                typeof(TCliCommand).Name, Environment.NewLine, error);
            Environment.ExitCode = 1;
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unhandled exception (Command: {Type}): {Message}",
                typeof(TCliCommand).Name, e.Message);
            Environment.ExitCode = 1;
        }

        _hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}