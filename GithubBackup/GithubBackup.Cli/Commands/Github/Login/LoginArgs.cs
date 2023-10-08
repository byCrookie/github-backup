using System.CommandLine;

namespace GithubBackup.Cli.Commands.Github.Login;

public class LoginArgs
{
    public string Token { get; }
    public bool DeviceFlowAuth { get; }

    public LoginArgs(string token, bool deviceFlowAuth)
    {
        Token = token;
        DeviceFlowAuth = deviceFlowAuth;
    }

    public static Option<string> TokenOption { get; }
    public static Option<bool> DeviceFlowAuthOption { get; }

    static LoginArgs()
    {
        TokenOption = new Option<string>(
            aliases: new[] { "-t", "--token" },
            description: LoginArgDescriptions.Token.Long
        ) { IsRequired = false };
        
        DeviceFlowAuthOption = new Option<bool>(
            aliases: new[] { "-dfa", "--device-flow-auth" },
            getDefaultValue: () => false,
            description: LoginArgDescriptions.DeviceFlowAuth.Long
        ) { IsRequired = false };
    }
}