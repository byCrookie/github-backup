namespace GithubBackup.Cli.Commands.Github.Login;

internal static class LoginArgDescriptions
{
    public static readonly Description Token = new("Token", "Token",
        """
        If not provided, the token will be aquired from the environment variable GITHUB_BACKUP_TOKEN.
        If provided, device flow authentication will be ignored. Recommended for use on servers.");
        """
    );

    public static readonly Description DeviceFlowAuth = new("DeviceFlowAuth", "Device Flow Auth",
        """
        Interactive authentication using the device flow.
        If a home directory is configured, the token will be stored in the file {home}/github-backup/token.
        Requires a browser. Recommended for use on clients.
        """
    );
}