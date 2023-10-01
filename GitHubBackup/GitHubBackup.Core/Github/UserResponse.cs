using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github;

public class UserResponse
{
    [JsonPropertyName("login")]
    public string Login { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }

    public UserResponse(string login, string name)
    {
        Login = login;
        Name = name;
    }
}