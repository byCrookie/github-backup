using System.CommandLine;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Download;

internal static class DownloadCommand
{
    private const string CommandName = "download";
    private const string CommandDescription = "Download migrations.";
    
    public static Command Create(string[] args)
    {
        var command = new Command(CommandName, CommandDescription);
        command.AddOptions(DownloadArgs.Options());
        
        command.SetHandler(
            (globalArgs, migrateArgs) => GithubBackup.Cli.Cli.RunAsync<DownloadRunner, DownloadArgs>(args, globalArgs, migrateArgs),
            new GlobalArgsBinder(),
            new DowndloadArgsBinder()
        );

        return command;
    }
}