using System.CommandLine;
using GitHubBackup.Cli;
using GitHubBackup.Cli.Commands;
using GitHubBackup.Cli.Github;
using GitHubBackup.Cli.Logging;
using GitHubBackup.Cli.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var rootCommand = new RootCommand("Github Backup");
rootCommand.AddGlobalOption(GlobalArgs.VerbosityOption);
rootCommand.AddGlobalOption(GlobalArgs.QuietOption);
rootCommand.AddGlobalOption(GlobalArgs.LogFileOption);
rootCommand.AddGlobalOption(GlobalArgs.InteractiveOption);

rootCommand.AddOption(GithubBackupArgs.DestinationOption);

rootCommand.SetHandler(
    (globalArgs, backupArgs) => RunAsync<IGithubBackup>(globalArgs, _ => new GithubBackup(globalArgs, backupArgs)),
    new GlobalArgsBinder(),
    new GithubBackupArgsBinder()
);

return await rootCommand.InvokeAsync(args);

Task RunAsync<TCliCommand>(GlobalArgs globalArgs, Func<IServiceProvider, TCliCommand> factory)
    where TCliCommand : class, ICliCommand
{
    Log.Logger = CliLoggerConfiguration.Create(globalArgs).CreateLogger();
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddTransient(factory);
    builder.Services.AddCli<TCliCommand>();
    var host = builder.Build();
    return host.RunAsync();
}