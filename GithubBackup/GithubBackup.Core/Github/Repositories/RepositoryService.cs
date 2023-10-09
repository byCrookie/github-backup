using Flurl;
using Flurl.Http;
using GithubBackup.Core.Github.Flurl;

namespace GithubBackup.Core.Github.Repositories;

internal sealed class RepositoryService : IRepositoryService
{
    public async Task<IReadOnlyCollection<Repository>> GetRepositoriesAsync(RepositoryOptions options,
        CancellationToken ct)
    {
        var request = new FlurlRequest("/user/repos");

        if (options.Type is not null)
        {
            request.SetQueryParam("type", options.Type);
        }
        else
        {
            request
                .SetQueryParam("affiliation", options.Affiliation)
                .SetQueryParam("visibility", options.Visibility);
        }

        var response = await request
            .GetJsonGithubApiPagedAsync<List<RepositoryResponse>, RepositoryResponse>(100, r => r, ct);

        return new List<Repository>(response.Select(r => new Repository(r.FullName)));
    }
}