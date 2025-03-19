using GithubBackup.Cli.Commands.Github.Auth.Pipeline;
using GithubBackup.Cli.Commands.Github.Login;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Core.Github.Users;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace GithubBackup.Cli.Commands.Github.Auth;

internal sealed class LoginService : ILoginService
{
    private readonly ILogger<LoginService> _logger;
    private readonly IAnsiConsole _ansiConsole;
    private readonly ILoginPipelineBuilder _loginPipelineBuilder;

    public LoginService(
        ILogger<LoginService> logger,
        IAnsiConsole ansiConsole,
        ILoginPipelineBuilder loginPipelineBuilder
    )
    {
        _logger = logger;
        _ansiConsole = ansiConsole;
        _loginPipelineBuilder = loginPipelineBuilder;
    }

    public async Task<User?> PersistentOnlyAsync(
        GlobalArgs globalArgs,
        LoginArgs args,
        CancellationToken ct
    )
    {
        var pipeline = _loginPipelineBuilder.PersistedOnly();
        var user = await pipeline.LoginAsync(globalArgs, args, false, ct);

        if (!globalArgs.Quiet && user is not null)
        {
            _ansiConsole.WriteLine($"Logged in as {user.Name}");
            _logger.LogInformation("Logged in as {Username}", user.Name);
        }

        return user;
    }

    public async Task<User> WithPersistentAsync(
        GlobalArgs globalArgs,
        LoginArgs args,
        bool persist,
        CancellationToken ct
    )
    {
        var pipeline = _loginPipelineBuilder.WithPersistent();
        var user = await pipeline.LoginAsync(globalArgs, args, persist, ct);

        if (user is null)
        {
            throw new Exception("Login failed");
        }

        if (!globalArgs.Quiet)
        {
            _ansiConsole.WriteLine($"Logged in as {user.Name}");
            _logger.LogInformation("Logged in as {Username}", user.Name);
        }

        return user;
    }

    public async Task<User> WithoutPersistentAsync(
        GlobalArgs globalArgs,
        LoginArgs args,
        bool persist,
        CancellationToken ct
    )
    {
        var pipeline = _loginPipelineBuilder.WithoutPersistent();
        var user = await pipeline.LoginAsync(globalArgs, args, persist, ct);

        if (user is null)
        {
            throw new Exception("Login failed");
        }

        if (!globalArgs.Quiet)
        {
            _ansiConsole.WriteLine($"Logged in as {user.Name}");
            _logger.LogInformation("Logged in as {Username}", user.Name);
        }

        return user;
    }
}
