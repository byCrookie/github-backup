using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Clients;
using GithubBackup.Core.Github.Credentials;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Repositories;
using GithubBackup.Core.Github.Users;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Github;

internal static class GithubModule
{
    public static void AddGithub(this IServiceCollection services)
    {
        services.AddMigration();
        services.AddAuthentication();
        services.AddUser();
        services.AddRepository();
        services.AddCredentials();
        services.AddClients();
    }
}
