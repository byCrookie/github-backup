using GithubBackup.Cli.Commands.Github.Auth.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Cli.Commands.Github.Auth;

internal static class AuthModule
{
    public static void AddAuth(this IServiceCollection services)
    {
        services.AddTransient<IPersistentCredentialStore, PersistentCredentialStore>();
        services.AddTransient<ILoginService, LoginService>();

        services.AddTransient<ILoginPipelineBuilder, LoginPipelineBuilder>();
        services.AddTransient<IDefaultPipeline, DefaultPipeline>();
        services.AddTransient<ITokenFromConfigurationPipeline, TokenFromConfigurationPipeline>();
        services.AddTransient<ITokenArgPipeline, TokenArgPipeline>();
        services.AddTransient<IPersistedPipeline, PersistedPipeline>();
        services.AddTransient<IDeviceFlowAuthPipeline, DeviceFlowAuthPipeline>();
    }
}
