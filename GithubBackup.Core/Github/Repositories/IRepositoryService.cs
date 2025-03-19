namespace GithubBackup.Core.Github.Repositories;

public interface IRepositoryService
{
    Task<IReadOnlyCollection<Repository>> GetRepositoriesAsync(
        RepositoryOptions options,
        CancellationToken ct
    );
}
