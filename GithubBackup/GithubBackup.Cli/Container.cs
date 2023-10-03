using System.ComponentModel;
using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Commands.Github;
using GithubBackup.Cli.Commands.Services;
using GithubBackup.Cli.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StrongInject;

namespace GithubBackup.Cli;

[RegisterModule(typeof(CliModule))]
internal partial class Container<TCliCommand, TCommandArgs> : IContainer<CliCommandService>, IContainer<Backup>
    where TCommandArgs : class
    where TCliCommand : class, ICliCommand
{
    private readonly IServiceProvider _serviceProvider;
    private readonly GlobalArgs _globalArgs;
    private readonly TCommandArgs _commandArgs;

    public Container(
        IServiceProvider serviceProvider,
        GlobalArgs globalArgs,
        TCommandArgs commandArgs)
    {
        _serviceProvider = serviceProvider;
        _globalArgs = globalArgs;
        _commandArgs = commandArgs;
    }

    // [Factory]
    // private ICliCommand GetCliCommand() => ActivatorUtilities
    //     .CreateInstance<TCliCommand>(GetService<IServiceProvider>(), _globalArgs, _commandArgs);

    [FactoryOf(typeof(ILogger<>)), FactoryOf(typeof(IHostApplicationLifetime))]
    private T GetService<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();
}