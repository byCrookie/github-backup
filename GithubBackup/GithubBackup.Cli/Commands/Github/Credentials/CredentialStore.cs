using System.IO.Abstractions;
using System.Security.Cryptography;
using GithubBackup.Core.Github.Credentials;
using static System.Text.Encoding;
using Convert = System.Convert;

namespace GithubBackup.Cli.Commands.Github.Credentials;

public class CredentialStore : ICredentialStore
{
    private readonly IFileSystem _fileSystem;
    
    private const string Key = "LBaZO3iFnF";
    private const string Salt = "fqCKmp5nwk";
    private const string TokenFileName = ".token";

    public CredentialStore(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public Task StoreTokenAsync(string accessToken, CancellationToken ct)
    {
        GithubTokenStore.Set(accessToken);
        var path = GetBackupPath();
        _fileSystem.Directory.CreateDirectory(path);
        var encryptedToken = EncryptString(accessToken, Key, Salt);
        return _fileSystem.File.WriteAllTextAsync(_fileSystem.Path.Combine(path, TokenFileName), encryptedToken, ct);
    }

    public async Task<string?> LoadTokenAsync(CancellationToken ct)
    {
        var path = GetBackupPath();
        var filePath = _fileSystem.Path.Combine(path, TokenFileName);

        if (_fileSystem.File.Exists(filePath))
        {
            var encryptedToken = await _fileSystem.File.ReadAllTextAsync(filePath, ct);
            var decryptedToken = DecryptString(encryptedToken, Key, Salt);
            GithubTokenStore.Set(decryptedToken);
            return decryptedToken;
        }

        GithubTokenStore.Set(null);
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
        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(plaintextBytes, 0, plaintextBytes.Length);
            }
            return Convert.ToBase64String(ms.ToArray());
        }
    }
 
    private static string DecryptString(string encrypted, string key, string salt)
    {
        var encryptedBytes = Convert.FromBase64String(encrypted);
        var saltBytes = UTF8.GetBytes(salt);
        var passwordBytes = new Rfc2898DeriveBytes(key, saltBytes, 20, HashAlgorithmName.SHA256);
        var encryptor = Aes.Create();
        encryptor.Key = passwordBytes.GetBytes(32);
        encryptor.IV = passwordBytes.GetBytes(16);
        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(encryptedBytes, 0, encryptedBytes.Length);
            }
            return UTF8.GetString(ms.ToArray());
        }
    }

    private string GetBackupPath()
    {
        return _fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GithubBackup");
    }
}