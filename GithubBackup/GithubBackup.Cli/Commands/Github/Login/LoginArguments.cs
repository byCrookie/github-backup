using System.CommandLine;

namespace GithubBackup.Cli.Commands.Github.Login;

public class LoginArguments
{
    public Option<string?> TokenOption { get; }
    public Option<bool> DeviceFlowAuthOption { get; }

    public LoginArguments()
    {
        TokenOption = new Option<string?>(
            aliases: new[] { "-t", "--token" },
            getDefaultValue: () => null,
            description: LoginArgDescriptions.Token.Long
        ) { IsRequired = false };
        
        DeviceFlowAuthOption = new Option<bool>(
            aliases: new[] { "-dfa", "--device-flow-auth" },
            getDefaultValue: () => false,
            description: LoginArgDescriptions.DeviceFlowAuth.Long
        ) { IsRequired = false };
    }
    
    public Option[] Options()
    {
        return new Option[]
        {
            TokenOption,
            DeviceFlowAuthOption
        };
    }
}