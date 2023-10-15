namespace GithubBackup.Core.Github.Credentials;

public class GithubTokenStore : IGithubTokenStore
{
    private string? _token;
    
    public void Set(string? token)
    {
        _token = token;
    }
    
    public string Get()
    {
        if (_token is null)
        {
            throw new InvalidOperationException("Token not set");
        }
        
        return _token;
    }
}