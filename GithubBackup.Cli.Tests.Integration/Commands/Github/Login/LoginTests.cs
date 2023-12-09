using GithubBackup.Core.Github.Users;

namespace GithubBackup.Cli.Tests.Integration.Commands.Github.Login;

[UsesVerify]
public class LoginTests
{
    [Theory]
    [InlineData("login --help")]
    [InlineData("login", 1)]
    public async Task RunAsync__(string args, int exitCode = 0)
    {
        await TestCli.RunAsync(args, exitCode, _ => {});
    }
    
    [Fact(Skip = "Not yet implemented")]
    public async Task RunAsync_LoginUsingToken_ThrowsException()
    {
        const string url = "https://api.github.com/user";
        
        const string args = "login --token ghp_7569254b-1521-4b1e-96ff-fc637f4e2f4d";

        await TestCli.RunAsync(args, 1, http =>
        {
            http
                .ForCallsTo(url)
                .WithVerb(HttpMethod.Get)
                .RespondWithJson(new UserResponse("user", "user"), headers: GetHeaders());
        });
    }
    
    private static Dictionary<string, string> GetHeaders(params KeyValuePair<string, string>[] headers)
    {
        var allHeaders = new Dictionary<string, string>
        {
            { "x-ratelimit-remaining", "4999" },
            { "x-ratelimit-reset", "1614556800" }
        };

        foreach (var header in headers)
        {
            allHeaders.Add(header.Key, header.Value);
        }

        return allHeaders;
    }
}