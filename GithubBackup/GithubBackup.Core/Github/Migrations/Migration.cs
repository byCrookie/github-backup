using GithubBackup.Core.Github.Repositories;

namespace GithubBackup.Core.Github.Migrations;

public class Migration
{
    public long Id { get; }
    public List<Repository> Repositories { get; }
    public MigrationState? State { get; }
    public string Url { get; }

    public Migration(long id, List<Repository> repositories, MigrationState? state, string url)
    {
        Id = id;
        Repositories = repositories;
        State = state;
        Url = url;
    }
}