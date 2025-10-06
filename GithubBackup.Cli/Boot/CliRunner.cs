using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GithubBackup.Cli.Boot;

internal class CliRunner<TCliCommand, TCommandArgs>(
    string[] args,
    GlobalArgs globalArgs,
    TCommandArgs commandArgs,
    RunOptions options)
    where TCommandArgs : class
    where TCliCommand : class, ICommandRunner
{
    public Task RunAsync(CancellationToken ct)
    {
        Log.Logger = CliLogger.Create(globalArgs).CreateLogger();

        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.AddEnvironmentVariables("GITHUB_BACKUP_");

        builder.Services.AddCli<TCliCommand, TCommandArgs>(globalArgs, commandArgs);
        options.AfterServices.Invoke(builder);

        var host = builder.Build();
        return host.RunAsync(ct);
    }
}
