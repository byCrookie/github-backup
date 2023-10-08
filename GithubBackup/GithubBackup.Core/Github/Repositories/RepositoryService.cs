using Flurl;
using GithubBackup.Core.Github.Flurl;

namespace GithubBackup.Core.Github.Repositories;

internal sealed class RepositoryService : IRepositoryService
{
    public async Task<IReadOnlyCollection<Repository>> GetRepositoriesAsync(CancellationToken ct)
    {
        var response = await "/user/repos"
            .SetQueryParam("affiliation", "owner")
            .GetJsonGithubApiPagedAsync<List<RepositoryResponse>, RepositoryResponse>(100, r => r, ct);

        return new List<Repository>(response.Select(r => new Repository(r.FullName)));
    }
}