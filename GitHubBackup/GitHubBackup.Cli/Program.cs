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
rootCommand.AddGlobalOption(GlobalArgs.VerbosityOption);
rootCommand.AddGlobalOption(GlobalArgs.QuietOption);
rootCommand.AddGlobalOption(GlobalArgs.LogFileOption);
rootCommand.AddGlobalOption(GlobalArgs.InteractiveOption);

rootCommand.AddOption(GithubBackupArgs.DestinationOption);

rootCommand.SetHandler(
    (globalArgs, backupArgs) => RunAsync<IBackup>(
        globalArgs,
        s => new Backup(globalArgs, backupArgs,
            s.GetRequiredService<IGithubService>(), s.GetRequiredService<ICredentialStore>())
    ),
    new GlobalArgsBinder(),
    new GithubBackupArgsBinder()
);

return await rootCommand.InvokeAsync(args);

Task RunAsync<TCliCommand>(GlobalArgs globalArgs, Func<IServiceProvider, TCliCommand> factory)
    where TCliCommand : class, ICliCommand
{
    try
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
    catch (Exception e)
    {
        Log.Fatal(e, "An unhandled exception occurred: {Message}", e.Message);
        return Task.CompletedTask;
    }
}