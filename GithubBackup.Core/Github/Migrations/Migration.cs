namespace GithubBackup.Core.Github.Migrations;

public sealed class Migration(long id, MigrationState? state, DateTime createdAt)
{
    public long Id { get; } = id;
    public MigrationState? State { get; } = state;
    public DateTime CreatedAt { get; } = createdAt;
}
