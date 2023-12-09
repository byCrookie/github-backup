using GithubBackup.Cli.Commands.Github.Auth;
using GithubBackup.Cli.Commands.Github.Backup;
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
        services.AddTransient<ManualBackupRunner>();
        services.AddTransient<MigrateRunner>();
        services.AddTransient<MigrationsRunner>();
        services.AddTransient<RepositoriesRunner>();
        services.AddTransient<BackupRunner>();
        services.AddTransient<LoginRunner>();
        services.AddTransient<DownloadRunner>();
        
        services.AddAuth();
    }
}