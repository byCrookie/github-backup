using System.CommandLine;
using GithubBackup.Cli.Options;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Download;

internal static class DownloadCommand
{
    private const string CommandName = "download";
    private const string CommandDescription = "Download migrations.";
    
    public static Command Create(Func<string[], GlobalArgs, DownloadArgs, Task> runAsync, string[] args)
    {
        var command = new Command(CommandName, CommandDescription);
        
        command.AddOptions(new List<Option>
        {
            DownloadArgs.MigrationsOption,
            DownloadArgs.LatestOption,
            DownloadArgs.DestinationOption,
            DownloadArgs.OverwriteOption
        });
        
        command.SetHandler(
            (globalArgs, migrateArgs) => runAsync(args, globalArgs, migrateArgs),
            new GlobalArgsBinder(),
            new DowndloadArgsBinder()
        );

        return command;
    }
}