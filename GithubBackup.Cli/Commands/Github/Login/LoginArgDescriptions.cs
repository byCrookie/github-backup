namespace GithubBackup.Cli.Commands.Github.Login;

internal static class LoginArgDescriptions
{
    public static readonly Description Token = new(
        "Token",
        "Token",
        """
        GitHub token to use for this command.
        Takes precedence over GITHUB_BACKUP_TOKEN, temporary cache, and device flow.
        """
    );

    public static readonly Description DeviceFlowAuth = new(
        "DeviceFlowAuth",
        "Device Flow Auth",
        """
        Start interactive GitHub device flow authentication.
        Bypasses the temporary token cache. Requires a browser.
        """
    );
}
