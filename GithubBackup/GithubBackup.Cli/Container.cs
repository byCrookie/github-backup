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
internal partial class Container<TCliCommand, TCliArguments> 
    : IContainer<CliCommandService<TCliCommand, TCliArguments>>, IContainer<TCliCommand>
    where TCliCommand : class, ICliCommand
    where TCliArguments : class
{
    private readonly IServiceProvider _serviceProvider;

    public Container(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    [Factory]
    private IServiceProvider GetServiceProvider() => _serviceProvider;

    [FactoryOf(typeof(ILogger<>)),
     FactoryOf(typeof(IHostApplicationLifetime)),
     FactoryOf(typeof(GlobalArgs)),
     FactoryOf(typeof(GithubBackupArgs))]
    private T GetService<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();
}