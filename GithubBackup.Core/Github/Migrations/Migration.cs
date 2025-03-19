namespace GithubBackup.Core.Github.Migrations;

public sealed class Migration
{
    public long Id { get; }
    public MigrationState? State { get; }
    public DateTime CreatedAt { get; }

    public Migration(long id, MigrationState? state, DateTime createdAt)
    {
        Id = id;
        State = state;
        CreatedAt = createdAt;
    }
}
