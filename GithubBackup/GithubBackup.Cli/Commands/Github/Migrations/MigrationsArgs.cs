namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsArgs
{
    public bool Long { get; }

    public MigrationsArgs(bool @long)
    {
        Long = @long;
    }
}