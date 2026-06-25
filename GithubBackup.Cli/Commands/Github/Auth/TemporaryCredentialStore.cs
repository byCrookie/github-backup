using System.IO.Abstractions;
using System.Text.Json;
using GithubBackup.Core.Environment;
using Microsoft.Extensions.Logging;
using Environment = System.Environment;

namespace GithubBackup.Cli.Commands.Github.Auth;

internal sealed class TemporaryCredentialStore : ITemporaryCredentialStore
{
    private readonly IFileSystem _fileSystem;
    private readonly IEnvironment _environment;
    private readonly ILogger<TemporaryCredentialStore> _logger;

    private const string TokenFileName = "temporary-token.json";
    private const string AppDirectory = "github-backup";

    public TemporaryCredentialStore(
        IFileSystem fileSystem,
        IEnvironment environment,
        ILogger<TemporaryCredentialStore> logger
    )
    {
        _fileSystem = fileSystem;
        _environment = environment;
        _logger = logger;
    }

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
        _logger.LogDebug("Storing temporary token in {File}", file);
        return _fileSystem.File.WriteAllTextAsync(file!, json, ct);
    }

    public async Task<TemporaryCredential?> LoadTokenAsync(CancellationToken ct)
    {
        if (!TryRetrieveTokenStoreFilePath(out var file))
        {
            return null;
        }

        if (!_fileSystem.File.Exists(file))
        {
            _logger.LogDebug("Temporary token file {File} does not exist", file);
            return null;
        }

        _logger.LogDebug("Loading temporary token from {File}", file);
        var json = await _fileSystem.File.ReadAllTextAsync(file, ct);

        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogWarning("Temporary token file {File} is empty", file);
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<TemporaryCredential>(json);
        }
        catch (JsonException e)
        {
            _logger.LogWarning(e, "Temporary token file {File} is invalid", file);
            return null;
        }
    }

    public Task DeleteTokenAsync(CancellationToken ct)
    {
        if (!TryRetrieveTokenStoreFilePath(out var file) || !_fileSystem.File.Exists(file))
        {
            return Task.CompletedTask;
        }

        _logger.LogDebug("Deleting temporary token from {File}", file);
        _fileSystem.File.Delete(file);
        return Task.CompletedTask;
    }

    private bool TryRetrieveTokenStoreFilePath(out string? path)
    {
        var appDataPath = _environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .FullName;
        _logger.LogDebug("AppData path is {AppDataPath}", appDataPath);

        if (!_fileSystem.Directory.Exists(appDataPath))
        {
            _logger.LogWarning("AppData path {Path} does not exist", appDataPath);
            path = null;
            return false;
        }

        var backupPath = _fileSystem.Path.Combine(appDataPath, AppDirectory);
        var tokenPath = _fileSystem.Path.Combine(backupPath, TokenFileName);
        _logger.LogDebug("Temporary token path is {TokenPath}", tokenPath);

        if (_fileSystem.Directory.Exists(backupPath))
        {
            _logger.LogDebug("Path {Path} exists", backupPath);
            path = tokenPath;
            return true;
        }

        try
        {
            _logger.LogDebug("Creating path {Path}", backupPath);
            _fileSystem.Directory.CreateDirectory(backupPath);
            path = tokenPath;
            return true;
        }
        catch (Exception)
        {
            _logger.LogWarning("Unable to create path {Path}", backupPath);
            path = null;
            return false;
        }
    }
}
