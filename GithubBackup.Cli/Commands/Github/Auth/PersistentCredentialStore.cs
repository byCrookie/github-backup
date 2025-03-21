﻿using System.IO.Abstractions;
using System.Security.Cryptography;
using GithubBackup.Core.Environment;
using Microsoft.Extensions.Logging;
using static System.Text.Encoding;
using Convert = System.Convert;
using Environment = System.Environment;

namespace GithubBackup.Cli.Commands.Github.Auth;

internal sealed class PersistentCredentialStore : IPersistentCredentialStore
{
    private readonly IFileSystem _fileSystem;
    private readonly IEnvironment _environment;
    private readonly ILogger<PersistentCredentialStore> _logger;

    private const string Key = "LBaZO3iFnF";
    private const string Salt = "fqCKmp5nwk";
    private const string TokenFileName = "token";
    private const string AppDirectory = "github-backup";

    public PersistentCredentialStore(
        IFileSystem fileSystem,
        IEnvironment environment,
        ILogger<PersistentCredentialStore> logger
    )
    {
        _fileSystem = fileSystem;
        _environment = environment;
        _logger = logger;
    }

    public Task StoreTokenAsync(string accessToken, CancellationToken ct)
    {
        if (!TryRetrieveTokenStoreFilePath(out var file))
        {
            throw new Exception(
                """
                Peristence of token failed. Unable to retrieve token store file path.
                Do not use login command, just provide login arguments to the specific command.
                """
            );
        }

        var encryptedToken = EncryptString(accessToken, Key, Salt);
        _logger.LogDebug("Storing encrypted token in {File}", file);
        return _fileSystem.File.WriteAllTextAsync(file!, encryptedToken, ct);
    }

    public async Task<string?> LoadTokenAsync(CancellationToken ct)
    {
        if (!TryRetrieveTokenStoreFilePath(out var file))
        {
            return null;
        }

        if (_fileSystem.File.Exists(file))
        {
            _logger.LogDebug("Loading encrypted token from {File}", file);
            var encryptedToken = await _fileSystem.File.ReadAllTextAsync(file, ct);

            if (string.IsNullOrWhiteSpace(encryptedToken))
            {
                _logger.LogWarning("Token file {File} is empty", file);
                return null;
            }

            return DecryptString(encryptedToken, Key, Salt);
        }

        _logger.LogDebug("Token file {File} does not exist", file);
        return null;
    }

    private static string EncryptString(string plaintext, string key, string salt)
    {
        var plaintextBytes = UTF8.GetBytes(plaintext);
        var saltBytes = UTF8.GetBytes(salt);
        var passwordBytes = new Rfc2898DeriveBytes(key, saltBytes, 20, HashAlgorithmName.SHA256);
        var encryptor = Aes.Create();
        encryptor.Key = passwordBytes.GetBytes(32);
        encryptor.IV = passwordBytes.GetBytes(16);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cs.Write(plaintextBytes, 0, plaintextBytes.Length);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    private static string DecryptString(string encrypted, string key, string salt)
    {
        var encryptedBytes = Convert.FromBase64String(encrypted);
        var saltBytes = UTF8.GetBytes(salt);
        var passwordBytes = new Rfc2898DeriveBytes(key, saltBytes, 20, HashAlgorithmName.SHA256);
        var encryptor = Aes.Create();
        encryptor.Key = passwordBytes.GetBytes(32);
        encryptor.IV = passwordBytes.GetBytes(16);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
        {
            cs.Write(encryptedBytes, 0, encryptedBytes.Length);
        }

        return UTF8.GetString(ms.ToArray());
    }

    private bool TryRetrieveTokenStoreFilePath(out string? path)
    {
        var appDataPath = _environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .FullName;
        _logger.LogDebug("AppData path is {AppDataPath}", appDataPath);

        if (_fileSystem.Directory.Exists(appDataPath))
        {
            var backupPath = _fileSystem.Path.Combine(appDataPath, AppDirectory);
            var tokenPath = _fileSystem.Path.Combine(backupPath, TokenFileName);
            _logger.LogDebug("Token path is {TokenPath}", tokenPath);

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

        _logger.LogWarning("AppData path {Path} does not exist", appDataPath);
        path = null;
        return false;
    }
}
