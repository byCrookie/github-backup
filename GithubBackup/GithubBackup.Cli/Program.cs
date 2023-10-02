using System.CommandLine;
using GithubBackup.Cli;
using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Github;
using GithubBackup.Cli.Github.Credentials;
using GithubBackup.Cli.Logging;
using GithubBackup.Cli.Options;
using GithubBackup.Core.Github;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var rootCommand = new RootCommand("Github Backup");

rootCommand.AddGlobalOptions(new List<Option>
{
    GlobalArgs.VerbosityOption,
    GlobalArgs.QuietOption,
    GlobalArgs.LogFileOption,
    GlobalArgs.InteractiveOption
});

rootCommand.AddOptions(new List<Option>
{
    GithubBackupArgs.DestinationOption
});

rootCommand.SetHandler(
    (globalArgs, backupArgs) => RunAsync<IBackup>(
        globalArgs,
        s => new Backup(globalArgs, backupArgs,
            s.GetRequiredService<IGithubService>(), s.GetRequiredService<ICredentialStore>())
    ),
    new GlobalArgsBinder(),
    new GithubBackupArgsBinder()
);

await rootCommand.InvokeAsync(args);
return Environment.ExitCode;

Task RunAsync<TCliCommand>(GlobalArgs globalArgs, Func<IServiceProvider, TCliCommand> factory)
    where TCliCommand : class, ICliCommand
{
    Log.Logger = CliLoggerConfiguration
        .Create(globalArgs)
        .CreateLogger();
        
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddTransient(factory);
    builder.Services.AddCli<TCliCommand>();
        
    var host = builder.Build();
    return host.RunAsync();
}