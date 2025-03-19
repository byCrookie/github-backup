using System.CommandLine;

namespace GithubBackup.Cli.Commands.Github.Login;

public class LoginArguments
{
    public Option<string?> TokenOption { get; } =
        new(aliases: new[] { "--token" }, description: LoginArgDescriptions.Token.Long)
        {
            IsRequired = false,
        };

    public Option<bool> DeviceFlowAuthOption { get; } =
        new(
            aliases: new[] { "--device-flow-auth" },
            getDefaultValue: () => false,
            description: LoginArgDescriptions.DeviceFlowAuth.Long
        )
        {
            IsRequired = false,
        };

    public IEnumerable<Option> Options()
    {
        return new Option[] { TokenOption, DeviceFlowAuthOption };
    }
}
