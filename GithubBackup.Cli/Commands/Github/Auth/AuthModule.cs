using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Commands.Github.Auth;

internal static class AuthModule
{
    public static void AddAuth(this IServiceCollection services)
    {
        services.AddTransient<ITemporaryCredentialStore, TemporaryCredentialStore>();
        services.AddTransient<ILoginService, LoginService>();
    }
}
