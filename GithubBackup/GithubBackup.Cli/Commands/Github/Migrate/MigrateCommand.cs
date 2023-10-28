using System.CommandLine;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Commands.Interval;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Migrate;

internal static class MigrateCommand
{
    private const string CommandName = "migrate";
    private const string CommandDescription = "Migrate a Github user.";
    
    public static Command Create(string[] args, GlobalArguments globalArguments)
    {
        var command = new Command(CommandName, CommandDescription);
        var migrateArguments = new MigrateArguments(true);
        var intervalArguments = new IntervalArguments();
        var loginArguments = new LoginArguments();
        command.AddOptions(migrateArguments.Options());
        command.AddOptions(intervalArguments.Options());
        command.AddOptions(loginArguments.Options());
        
        command.SetHandler(
            (globalArgs, migrateArgs) => GithubBackup.Cli.Cli.RunAsync<MigrateRunner, MigrateArgs>(args, globalArgs, migrateArgs),
            new GlobalArgsBinder(globalArguments),
            new MigrateArgsBinder(migrateArguments, intervalArguments, loginArguments)
        );

        return command;
    }
}