using System.IO.Abstractions;
using System.Text.Json;
using GithubBackup.Core.Environment;
using Microsoft.Extensions.Logging;
using Environment = System.Environment;

namespace GithubBackup.Cli.Commands.Github.Auth;

internal sealed class TemporaryCredentialStore(
    IFileSystem fileSystem,
    IEnvironment environment,
    ILogger<TemporaryCredentialStore> logger
) : ITemporaryCredentialStore
{
    private const string TokenFileName = "temporary-token.json";
    private const string AppDirectory = "github-backup";

    public Task StoreTokenAsync(string accessToken, DateTimeOffset? expiresAt, CancellationToken ct)
    {
        if (!TryRetrieveTokenStoreFilePath(out var file))
        {
            throw new Exception(
                "Unable to temporarily store token. Provide --token or GITHUB_BACKUP_TOKEN instead."
            );
        }

        var credential = new TemporaryCredential(accessToken, expiresAt);
        var json = JsonSerializer.Serialize(credential);
        logger.LogDebug("Storing temporary token in {File}", file);
        return fileSystem.File.WriteAllTextAsync(file!, json, ct);
    }

    public async Task<TemporaryCredential?> LoadTokenAsync(CancellationToken ct)
    {
        if (!TryRetrieveTokenStoreFilePath(out var file))
        {
            return null;
        }

        if (!fileSystem.File.Exists(file))
        {
            logger.LogDebug("Temporary token file {File} does not exist", file);
            return null;
        }

        logger.LogDebug("Loading temporary token from {File}", file);
        var json = await fileSystem.File.ReadAllTextAsync(file, ct);

        if (string.IsNullOrWhiteSpace(json))
        {
            logger.LogWarning("Temporary token file {File} is empty", file);
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<TemporaryCredential>(json);
        }
        catch (JsonException e)
        {
            logger.LogWarning(e, "Temporary token file {File} is invalid", file);
            return null;
        }
    }

    public Task DeleteTokenAsync(CancellationToken ct)
    {
        if (!TryRetrieveTokenStoreFilePath(out var file) || !fileSystem.File.Exists(file))
        {
            return Task.CompletedTask;
        }

        logger.LogDebug("Deleting temporary token from {File}", file);
        fileSystem.File.Delete(file);
        return Task.CompletedTask;
    }

    private bool TryRetrieveTokenStoreFilePath(out string? path)
    {
        var appDataPath = environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .FullName;
        logger.LogDebug("AppData path is {AppDataPath}", appDataPath);

        if (!fileSystem.Directory.Exists(appDataPath))
        {
            logger.LogWarning("AppData path {Path} does not exist", appDataPath);
            path = null;
            return false;
        }

        var backupPath = fileSystem.Path.Combine(appDataPath, AppDirectory);
        var tokenPath = fileSystem.Path.Combine(backupPath, TokenFileName);
        logger.LogDebug("Temporary token path is {TokenPath}", tokenPath);

        if (fileSystem.Directory.Exists(backupPath))
        {
            logger.LogDebug("Path {Path} exists", backupPath);
            path = tokenPath;
            return true;
        }

        try
        {
            logger.LogDebug("Creating path {Path}", backupPath);
            fileSystem.Directory.CreateDirectory(backupPath);
            path = tokenPath;
            return true;
        }
        catch (Exception)
        {
            logger.LogWarning("Unable to create path {Path}", backupPath);
            path = null;
            return false;
        }
    }
}
