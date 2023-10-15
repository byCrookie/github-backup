using Flurl.Http;
using GithubBackup.Core.Github.Credentials;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Flurl;

internal static class FlurlModule
{
    public static void AddFlurl(this IServiceCollection _)
    {
        FlurlHttp.Configure(settings => settings.JsonSerializer = new TextJsonSerializer());
    }
}