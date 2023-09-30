using System.Security.Cryptography;
using Octokit;

namespace GithubBackup.Cli.Github.GithubCredentials;

public class AppSettingsCredentialsesStore : ICredentialStore, IAppSettingsCredentialsStore
{
    private const string Key = "LBaZO3iFnF";
    private const string Salt = "fqCKmp5nwk";
    private const string TokenFileName = ".token";
    private const string UserFileName = ".user";
        
    public async Task<Credentials> GetCredentials()
    {
        var token = await LoadTokenAsync(CancellationToken.None);
        var exception = new Exception($"Populate the credential store first using {nameof(IAppSettingsCredentialsStore)}");
        return !string.IsNullOrWhiteSpace(token) ? new Credentials(token) : throw exception;
    }

    public Task StoreUsernameAsync(string user, CancellationToken ct)
    {
        var path = GetPath();
        Directory.CreateDirectory(path);
        return File.WriteAllTextAsync(Path.Combine(path, UserFileName), user, ct);
    }

    public async Task<string?> LoadUsernameAsync(CancellationToken ct)
    {
        var path = GetPath();
        var filePath = Path.Combine(path, UserFileName);

        if (File.Exists(filePath))
        {
            return await File.ReadAllTextAsync(filePath, ct);
        }

        return null;
    }

    public Task StoreTokenAsync(string accessToken, CancellationToken ct)
    {
        var path = GetPath();
        Directory.CreateDirectory(path);
        var encryptedToken = EncryptString(accessToken, Key, Salt);
        return File.WriteAllTextAsync(Path.Combine(path, TokenFileName), encryptedToken, ct);
    }

    public async Task<string?> LoadTokenAsync(CancellationToken ct)
    {
        var path = GetPath();
        var filePath = Path.Combine(path, TokenFileName);

        if (File.Exists(filePath))
        {
            var encryptedToken = await File.ReadAllTextAsync(filePath, ct);
            return DecryptString(encryptedToken, Key, Salt);
        }

        return null;
    }

    private static string EncryptString(string plaintext, string key, string salt)
    {
        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var saltBytes = System.Text.Encoding.UTF8.GetBytes(salt);
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
        var saltBytes = System.Text.Encoding.UTF8.GetBytes(salt);
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
            return System.Text.Encoding.UTF8.GetString(ms.ToArray());
        }
    }

    private static string GetPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GithubBackup");
    }
}