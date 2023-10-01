namespace GithubBackup.Core.Github.Credentials;

public static class GithubTokenStore
{
    private static string? _token;
    
    public static void Set(string? token)
    {
        _token = token;
    }
    
    public static string Get()
    {
        if (_token is null)
        {
            throw new InvalidOperationException("Token not set");
        }
        
        return _token;
    }
}