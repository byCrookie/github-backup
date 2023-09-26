namespace GitHubBackup.Cli.Github;

internal interface IGithubBackup
{
    public Task BackupAsync();
}