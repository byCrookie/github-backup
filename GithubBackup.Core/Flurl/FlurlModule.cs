using Flurl.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Flurl;

internal static class FlurlModule
{
    public static void AddFlurl(this IServiceCollection _)
    {
        FlurlHttp.Clients.WithDefaults(b => b.Settings.JsonSerializer  = new TextJsonSerializer());
    }
}