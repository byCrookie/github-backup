﻿using Flurl;
using Flurl.Http;
using GithubBackup.Core.Github.Clients;

namespace GithubBackup.Core.Github.Repositories;

internal sealed class RepositoryService : IRepositoryService
{
    private readonly IGithubApiClient _githubApiClient;

    public RepositoryService(IGithubApiClient githubApiClient)
    {
        _githubApiClient = githubApiClient;
    }

    public Task<IReadOnlyCollection<Repository>> GetRepositoriesAsync(RepositoryOptions options, CancellationToken ct)
    {
        if (options.Type is not null)
        {
            return GetRepositoryResponseAsync(rq => rq.SetQueryParam("type", options.Type), ct);
        }

        return GetRepositoryResponseAsync(
            rq => rq.SetQueryParam("affiliation", options.Affiliation).SetQueryParam("visibility", options.Visibility),
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