using System.CommandLine;

namespace GithubBackup.Cli.Commands.Github.Login;

internal sealed class LoginArgsBinder(LoginArguments loginArguments)
{
    public LoginArgs Get(ParseResult parseResult)
    {
        var token = parseResult.GetValue(loginArguments.TokenOption);
        var deviceFlowAuth = parseResult.GetValue(
            loginArguments.DeviceFlowAuthOption
        );

        return new LoginArgs(token, deviceFlowAuth);
    }
}
