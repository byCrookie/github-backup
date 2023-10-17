using System.Net;
using FluentAssertions;
using Flurl;
using Flurl.Http;
using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Commands.Services;
using GithubBackup.TestUtils.Logging;
using Meziantou.Xunit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GithubBackup.Cli.Tests.Commands.Services;

[DisableParallelization]
public class CliCommandServiceTests
{
    private readonly CliCommandService _sut;
    private readonly ILogger<CliCommandService> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifeTime;
    private readonly ICommandRunner _commandRunner;

    public CliCommandServiceTests()
    {
        _logger = Substitute.For<ILogger<CliCommandService>>();
        _hostApplicationLifeTime = Substitute.For<IHostApplicationLifetime>();
        _commandRunner = Substitute.For<ICommandRunner>();

        _sut = new CliCommandService(_logger, _hostApplicationLifeTime, _commandRunner);
    }

    [Fact]
    public async Task StartAsync_Runner_IsRunAndClosed()
    {
        await _sut.StartAsync(CancellationToken.None);

        _hostApplicationLifeTime.Received(1).StopApplication();
        await _commandRunner.Received(1).RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Starting command: (.*)")
        );

        Environment.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task StartAsync_RunnerFails_ExceptionIsHandled()
    {
        const string errorMessage = "Test";
        _commandRunner.RunAsync(CancellationToken.None).ThrowsAsync(new Exception(errorMessage));

        await _sut.StartAsync(CancellationToken.None);

        _hostApplicationLifeTime.Received(1).StopApplication();
        await _commandRunner.Received(1).RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Starting command: (.*)"),
            new LogEntry(LogLevel.Critical, "Unhandled exception [(]Command: (.*)[)]: Test")
        );

        Environment.ExitCode.Should().Be(1);
    }

    [Fact]
    public async Task StartAsync_RunnerFailsWithFlurl_ExceptionIsHandled()
    {
        const string errorMessage = "Test";
        _commandRunner.RunAsync(CancellationToken.None).ThrowsAsync(CreateFlurlHttpException(errorMessage));

        await _sut.StartAsync(CancellationToken.None);

        _hostApplicationLifeTime.Received(1).StopApplication();
        await _commandRunner.Received(1).RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Starting command: (.*)"),
            new LogEntry(LogLevel.Critical, "Unhandled exception [(]Command: (.*)[)]: Test")
        );

        Environment.ExitCode.Should().Be(1);
    }

    private static FlurlHttpException CreateFlurlHttpException(string errorMessage)
    {
        return new FlurlHttpException(new FlurlCall
        {
            Exception = new Exception(errorMessage),
            Response = new FlurlResponse(new HttpResponseMessage
            {
                ReasonPhrase = errorMessage,
                Content = new StringContent(errorMessage),
                StatusCode = HttpStatusCode.BadRequest
            }),
            Request = new FlurlRequest(new Url("https://example.com")),
            HttpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com"),
            HttpResponseMessage = new HttpResponseMessage
            {
                ReasonPhrase = errorMessage,
                Content = new StringContent("error"),
                StatusCode = HttpStatusCode.BadRequest
            }
        });
    }
}