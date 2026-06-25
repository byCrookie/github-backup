namespace GithubBackup.Cli.Commands.Github.Auth;

internal interface ITemporaryCredentialStore
{
    Task StoreTokenAsync(string accessToken, DateTimeOffset? expiresAt, CancellationToken ct);

    Task<TemporaryCredential?> LoadTokenAsync(CancellationToken ct);

    Task DeleteTokenAsync(CancellationToken ct);
}
