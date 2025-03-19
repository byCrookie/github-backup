using Flurl;
using Flurl.Http;

namespace GithubBackup.Core.Github.Clients;

public interface IGithubWebClient
{
    Task<IFlurlResponse> PostJsonAsync(
        Url url,
        object data,
        Action<IFlurlRequest>? configure = null,
        CancellationToken? ct = null
    );
}
