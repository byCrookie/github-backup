using System.CommandLine;

namespace GithubBackup.Cli.Commands.Github.Login;

public class LoginArguments
{
    public Option<string?> TokenOption { get; } =
        new(
            name: "--token",
            aliases: ["-t"]
        )
        {
            Required = false,
            Description = LoginArgDescriptions.Token.Long
        };

    public Option<bool> DeviceFlowAuthOption { get; } =
        new(
            name: "--device-flow-auth",
            aliases: ["-d"]
        )
        {
            Required = false,
            Description = LoginArgDescriptions.DeviceFlowAuth.Long,
            DefaultValueFactory = _ => false
        };

    public IEnumerable<Option> Options()
    {
        return [TokenOption, DeviceFlowAuthOption];
    }
}
