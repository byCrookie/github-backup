using GithubBackup.Cli.Commands.Github.Backup;
using GithubBackup.Cli.Commands.Github.Credentials;
using GithubBackup.Cli.Commands.Github.Download;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Github.Manual;
using GithubBackup.Cli.Commands.Github.Migrate;
using GithubBackup.Cli.Commands.Github.Migrations;
using GithubBackup.Cli.Commands.Github.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Commands.Github;

internal static class GithubModule
{
    public static void AddGithub(this IServiceCollection services)
    {
        services.AddTransient<IManualBackup, ManualBackup>();
        services.AddTransient<IMigrate, Migrate.Migrate>();
        services.AddTransient<IMigrations, Migrations.Migrations>();
        services.AddTransient<IRepositories, Repositories.Repositories>();
        services.AddTransient<IBackup, Backup.Backup>();
        services.AddTransient<ILogin, Login.Login>();
        services.AddTransient<IDownload, Download.Download>();
        
        services.AddCredentials();
    }
}