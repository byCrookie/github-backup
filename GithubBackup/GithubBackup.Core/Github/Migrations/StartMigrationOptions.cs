namespace GithubBackup.Core.Github.Migrations;

public class StartMigrationOptions
{
    public IReadOnlyCollection<string> Repositories { get; }
    
    public StartMigrationOptions(IReadOnlyCollection<string> repositories)
    {
        Repositories = repositories;
    }
}