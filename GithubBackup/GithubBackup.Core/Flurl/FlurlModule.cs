using Flurl.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Flurl;

internal static class FlurlModule
{
    // ReSharper disable once UnusedParameter.Global
    public static void AddFlurl(this IServiceCollection _)
    {
        FlurlHttp.Configure(settings => settings.JsonSerializer = new TextJsonSerializer());
    }
}