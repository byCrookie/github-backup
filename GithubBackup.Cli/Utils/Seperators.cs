namespace GithubBackup.Cli.Utils;

public record Seperators(string[] Values)
{
    public override string ToString()
    {
        return string.Join(" or ", Values);
    }
};
