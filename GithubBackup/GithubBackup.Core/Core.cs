using Flurl.Http;
using GithubBackup.Core.Flurl;

namespace GithubBackup.Core;

public static class Core
{
    public static void Initialize()
    {
        FlurlHttp.Configure(settings => settings.JsonSerializer = new TextJsonSerializer());
    }
}