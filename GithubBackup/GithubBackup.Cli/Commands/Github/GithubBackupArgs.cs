using System.CommandLine;

namespace GithubBackup.Cli.Commands.Github;

internal class GithubBackupArgs
{
    public DirectoryInfo Destination { get; }

    public GithubBackupArgs(DirectoryInfo destination)
    {
        Destination = destination;
    }
    
    public static Option<DirectoryInfo> DestinationOption { get; } = new(
        aliases: new[] { "-d", "--destination" },
        getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory()),
        description: "The path to put the backup in"
    ) { IsRequired = true };
}