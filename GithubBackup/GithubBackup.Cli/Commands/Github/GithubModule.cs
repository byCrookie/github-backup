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
        services.AddTransient<IManualBackupRunner, ManualBackupRunner>();
        services.AddTransient<IMigrateRunner, Migrate.MigrateRunner>();
        services.AddTransient<IMigrationsRunner, Migrations.MigrationsRunner>();
        services.AddTransient<IRepositoriesRunner, Repositories.RepositoriesRunner>();
        services.AddTransient<IBackupRunner, Backup.BackupRunner>();
        services.AddTransient<ILoginRunner, Login.LoginRunner>();
        services.AddTransient<IDownloadRunner, Download.DownloadRunner>();
        
        services.AddCredentials();
    }
}