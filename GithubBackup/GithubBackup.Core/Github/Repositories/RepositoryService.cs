using Flurl.Http;
using GithubBackup.Core.Github.Clients;
using GithubBackup.Core.Utils;
using Microsoft.Extensions.Logging;

namespace GithubBackup.Core.Github.Repositories;

internal sealed class RepositoryService : IRepositoryService
{
    private readonly IGithubApiClient _githubApiClient;
    private readonly ILogger<RepositoryService> _logger;

    public RepositoryService(IGithubApiClient githubApiClient, ILogger<RepositoryService> logger)
    {
        _githubApiClient = githubApiClient;
        _logger = logger;
    }

    public Task<IReadOnlyCollection<Repository>> GetRepositoriesAsync(RepositoryOptions options, CancellationToken ct)
    {
        if (options.Type is not null)
        {
            _logger.LogInformation("Getting repositories of type {Type}", options.Type.Value);
            return GetRepositoryResponseAsync(rq => rq.SetQueryParam("type", options.Type.Value.GetEnumMemberValue()), ct);
        }

        _logger.LogInformation("Getting repositories of affiliation {Affiliation} and visibility {Visibility}",
            options.Affiliation!.Value, options.Visibility!.Value);
        return GetRepositoryResponseAsync(
            rq => rq
                .SetQueryParam("affiliation", options.Affiliation!.Value.GetEnumMemberValue())
                .SetQueryParam("visibility", options.Visibility!.Value.GetEnumMemberValue()),
            ct
        );
    }

    private async Task<IReadOnlyCollection<Repository>> GetRepositoryResponseAsync(Action<IFlurlRequest> configure,
        CancellationToken ct)
    {
        var response = await _githubApiClient.ReceiveJsonPagedAsync<List<RepositoryResponse>, RepositoryResponse>(
            "/user/repos",
            100,
            r => r,
            configure,
            ct
        );

        return new List<Repository>(response.Select(r => new Repository(r.FullName)));
    }
}