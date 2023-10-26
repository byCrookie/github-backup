namespace GithubBackup.Cli.Commands.Github.Login;

internal sealed class LoginArgs
{
    public string? Token { get; }
    public bool DeviceFlowAuth { get; }

    public LoginArgs(string? token, bool deviceFlowAuth)
    {
        Token = token;
        DeviceFlowAuth = deviceFlowAuth;
    }
}