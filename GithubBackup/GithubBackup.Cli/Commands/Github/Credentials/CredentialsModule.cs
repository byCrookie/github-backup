using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Commands.Github.Credentials;

internal static class CredentialsModule
{
    public static void AddCredentials(this IServiceCollection services)
    {
        services.AddTransient<ICredentialStore, CredentialStore>();
    }
}