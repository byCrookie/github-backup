using System.Net;
using AwesomeAssertions;
using GithubBackup.Core.Github.Users;

namespace GithubBackup.Cli.Tests.Integration.Commands.Github.Login;

public class LoginTests
{
    [Theory]
    [InlineData("login --help")]
    public async Task RunAsync__(string args, int exitCode = 0)
    {
        var action = () => TestCli.RunAsync(args, exitCode, _ => { });
        if (exitCode == 0)
        {
            await action();
        }
        else
        {
            await action.Should().ThrowAsync<Exception>().WithMessage("Authentication failed.");
        }
    }

    [Fact]
    public async Task RunAsync_LoginUsingDeviceFlow_Succeeds()
    {
        await TestCli.RunAsync(
            "login",
            0,
            http =>
            {
                http.ForCallsTo("https://github.com/login/device/code")
                    .WithVerb(HttpMethod.Post)
                    .RespondWithJson(
                        new
                        {
                            device_code = "device",
                            user_code = "USER-CODE",
                            verification_uri = "https://github.com/login/device",
                            expires_in = 900,
                            interval = 5,
                        }
                    );
                http.ForCallsTo("https://github.com/login/oauth/access_token")
                    .WithVerb(HttpMethod.Post)
                    .RespondWithJson(
                        new
                        {
                            access_token = "device-token",
                            token_type = "bearer",
                            scope = string.Empty,
                            expires_in = 60,
                        }
                    );
                http.ForCallsTo("https://api.github.com/user")
                    .WithVerb(HttpMethod.Get)
                    .RespondWithJson(new UserResponse("user", "user"), headers: GetHeaders());
            },
            fs =>
                fs.Directory.CreateDirectory(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                )
        );
    }

    [Fact]
    public async Task RunAsync_LoginUsingToken_Succeeds()
    {
        const string url = "https://api.github.com/user";

        const string args = "login --token ghp_7569254b-1521-4b1e-96ff-fc637f4e2f4d";

        await TestCli.RunAsync(
            args,
            0,
            http =>
            {
                http.ForCallsTo(url)
                    .WithVerb(HttpMethod.Get)
                    .RespondWithJson(new UserResponse("user", "user"), headers: GetHeaders());
            }
        );
    }

    [Fact]
    public async Task RunAsync_LoginUsingToken_ThrowsException()
    {
        const string url = "https://api.github.com/user";

        const string args = "login --token ghp_7569254b-1521-4b1e-96ff-fc637f4e2f4d";

        var action = async () =>
            await TestCli.RunAsync(
                args,
                1,
                http =>
                {
                    http.ForCallsTo(url)
                        .WithVerb(HttpMethod.Get)
                        .RespondWithJson(
                            new UserResponse("user", "user"),
                            (int)HttpStatusCode.BadRequest,
                            headers: GetHeaders()
                        );
                }
            );

        await action.Should().ThrowAsync<Exception>().WithMessage("GitHub token is invalid.");
    }

    private static Dictionary<string, string> GetHeaders(
        params KeyValuePair<string, string>[] headers
    )
    {
        var allHeaders = new Dictionary<string, string>
        {
            { "x-ratelimit-remaining", "4999" },
            { "x-ratelimit-reset", "1614556800" },
        };

        foreach (var header in headers)
        {
            allHeaders.Add(header.Key, header.Value);
        }

        return allHeaders;
    }
}
