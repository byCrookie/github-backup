using System.CommandLine;
using GithubBackup.Cli.Options;

namespace GithubBackup.Cli.Commands.Github.Login;

internal static class LoginCommand
{
    private const string CommandName = "login";
    private const string CommandDescription = "Login to Github.";
    
    public static Command Create(Func<string[], GlobalArgs, LoginArgs, Task> runAsync, string[] args)
    {
        var migrateCommand = new Command(CommandName, CommandDescription);
        
        migrateCommand.AddOptions(new List<Option>
        {
            LoginArgs.TokenOption,
            LoginArgs.DeviceFlowAuthOption
        });
        
        migrateCommand.SetHandler(
            (globalArgs, loginArgs) => runAsync(args, globalArgs, loginArgs),
            new GlobalArgsBinder(),
            new LoginArgsBinder()
        );

        return migrateCommand;
    }
}