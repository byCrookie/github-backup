using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github;

public class UserResponse
{
    [JsonPropertyName("login")]
    public string Login { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; }

    public UserResponse(string login, string name, string email)
    {
        Login = login;
        Name = name;
        Email = email;
    }
}