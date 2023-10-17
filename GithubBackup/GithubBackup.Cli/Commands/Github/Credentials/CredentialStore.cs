using System.IO.Abstractions;
using System.Security.Cryptography;
using GithubBackup.Core.Environment;
using GithubBackup.Core.Github.Credentials;
using static System.Text.Encoding;
using Convert = System.Convert;
using Environment = System.Environment;

namespace GithubBackup.Cli.Commands.Github.Credentials;

internal sealed class CredentialStore : ICredentialStore
{
    private readonly IFileSystem _fileSystem;
    private readonly IGithubTokenStore _githubTokenStore;
    private readonly IEnvironment _environment;

    private const string Key = "LBaZO3iFnF";
    private const string Salt = "fqCKmp5nwk";
    private const string TokenFileName = "token";

    public CredentialStore(
        IFileSystem fileSystem,
        IGithubTokenStore githubTokenStore,
        IEnvironment environment)
    {
        _fileSystem = fileSystem;
        _githubTokenStore = githubTokenStore;
        _environment = environment;
    }

    public Task StoreTokenAsync(string accessToken, CancellationToken ct)
    {
        if (!TryRetrieveTokenStoreFilePath(out var file))
        {
            return Task.CompletedTask;
        }

        _githubTokenStore.Set(accessToken);

        var encryptedToken = EncryptString(accessToken, Key, Salt);
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
            var encryptedToken = await _fileSystem.File.ReadAllTextAsync(file, ct);
            var decryptedToken = DecryptString(encryptedToken, Key, Salt);
            _githubTokenStore.Set(decryptedToken);
            return decryptedToken;
        }

        _githubTokenStore.Set(null);
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
        var appDataPath = _environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).FullName;

        if (_fileSystem.Directory.Exists(appDataPath))
        {
            var backupPath = _fileSystem.Path.Combine(appDataPath, "GithubBackup");
            var tokenPath = _fileSystem.Path.Combine(backupPath, TokenFileName);

            if (_fileSystem.Directory.Exists(backupPath))
            {
                path = tokenPath;
                return true;
            }

            try
            {
                _fileSystem.Directory.CreateDirectory(backupPath);
                path = tokenPath;
                return true;
            }
            catch (Exception)
            {
                path = null;
                return false;
            }
        }

        path = null;
        return false;
    }
}