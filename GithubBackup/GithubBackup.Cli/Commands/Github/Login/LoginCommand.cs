using System.CommandLine;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Login;

internal static class LoginCommand
{
    private const string CommandName = "login";
    private const string CommandDescription = "Login to Github.";
    
    public static Command Create(Func<string[], GlobalArgs, LoginArgs, Task> runAsync, string[] args)
    {
        var command = new Command(CommandName, CommandDescription);
        
        command.AddOptions(new List<Option>
        {
            LoginArgs.TokenOption,
            LoginArgs.DeviceFlowAuthOption
        });
        
        command.SetHandler(
            (globalArgs, loginArgs) => runAsync(args, globalArgs, loginArgs),
            new GlobalArgsBinder(),
            new LoginArgsBinder()
        );

        return command;
    }
}