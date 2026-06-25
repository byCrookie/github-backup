namespace GithubBackup.Cli.Commands.Github.Login;

internal static class LoginArgDescriptions
{
    public static readonly Description Token = new(
        "Token",
        "Token",
        """
        If not provided, the token will be acquired from the environment variable GITHUB_BACKUP_TOKEN.
        If provided, temporary cache and device flow authentication will be ignored. Recommended for use on servers.
        """
    );

    public static readonly Description DeviceFlowAuth = new(
        "DeviceFlowAuth",
        "Device Flow Auth",
        """
        Interactive authentication using the device flow.
        Bypasses the temporary token cache. Requires a browser. Recommended for use on clients.
        """
    );
}
