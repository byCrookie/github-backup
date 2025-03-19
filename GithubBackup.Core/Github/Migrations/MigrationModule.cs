using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Github.Migrations;

internal static class MigrationModule
{
    public static void AddMigration(this IServiceCollection services)
    {
        services.AddTransient<IMigrationService, MigrationService>();
    }
}
