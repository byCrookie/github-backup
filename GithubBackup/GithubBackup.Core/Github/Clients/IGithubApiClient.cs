using Flurl;
using Flurl.Http;

namespace GithubBackup.Core.Github.Clients;

public interface IGithubApiClient
{
    Task<List<TItem>> ReceiveJsonPagedAsync<TReponse, TItem>(Url url, int perPage, Func<TReponse, List<TItem>> getItems,
        Action<IFlurlRequest>? configure = null, CancellationToken? ct = null);

    Task<string> DownloadFileAsync(Url url, string path, string? fileName = null,
        Action<IFlurlRequest>? configure = null, CancellationToken? ct = null);

    Task<IFlurlResponse> GetAsync(Url url, Action<IFlurlRequest>? configure = null, CancellationToken? ct = null);

    Task<IFlurlResponse> PostJsonAsync(Url url, object data, Action<IFlurlRequest>? configure = null,
        CancellationToken? ct = null);
}