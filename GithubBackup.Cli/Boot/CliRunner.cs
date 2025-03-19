using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GithubBackup.Cli.Boot;

internal class CliRunner<TCliCommand, TCommandArgs>
    where TCommandArgs : class
    where TCliCommand : class, ICommandRunner
{
    private readonly string[] _args;
    private readonly GlobalArgs _globalArgs;
    private readonly TCommandArgs _commandArgs;
    private readonly RunOptions _options;

    public CliRunner(
        string[] args,
        GlobalArgs globalArgs,
        TCommandArgs commandArgs,
        RunOptions options
    )
    {
        _args = args;
        _globalArgs = globalArgs;
        _commandArgs = commandArgs;
        _options = options;
    }

    public Task RunAsync()
    {
        Log.Logger = CliLogger.Create(_globalArgs).CreateLogger();

        var builder = Host.CreateApplicationBuilder(_args);

        builder.Configuration.AddEnvironmentVariables("GITHUB_BACKUP_");

        builder.Services.AddCli<TCliCommand, TCommandArgs>(_globalArgs, _commandArgs);
        _options.AfterServices.Invoke(builder);

        var host = builder.Build();
        return host.RunAsync();
    }
}
