using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Flurl;
using Flurl.Http;
using GithubBackup.Cli.Commands;
using GithubBackup.Cli.Commands.Global;
using GithubBackup.Cli.Commands.Services;
using GithubBackup.Core.Environment;
using GithubBackup.Core.Utils;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Spectre.Console.Testing;

namespace GithubBackup.Cli.Tests.Commands.Services;

[UsesVerify]
public class CommandRunnerServiceTests
{
    private readonly CommandRunnerService _sut;
    private readonly ILogger<CommandRunnerService> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifeTime;
    private readonly ICommandRunner _commandRunner;
    private readonly TestConsole _ansiConsole;

    public CommandRunnerServiceTests()
    {
        _logger = Substitute.For<ILogger<CommandRunnerService>>();
        _hostApplicationLifeTime = Substitute.For<IHostApplicationLifetime>();
        _commandRunner = Substitute.For<ICommandRunner>();
        _ansiConsole = new TestConsole();
        var stopwatch = Substitute.For<IStopwatch>();

        var fakeStopWatch = new Stopwatch();
        stopwatch.StartNew().Returns(fakeStopWatch);

        _sut = new CommandRunnerService(
            new GlobalArgs(LogLevel.Debug, false, new FileInfo("test")),
            _logger,
            _hostApplicationLifeTime,
            _commandRunner,
            _ansiConsole,
            stopwatch
        );
    }

    [Fact]
    public async Task StartAsync_Runner_IsRunAndClosed()
    {
        await _sut.StartAsync(CancellationToken.None);

        _hostApplicationLifeTime.Received(1).StopApplication();
        await _commandRunner.Received(1).RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Running command"),
            new LogEntry(LogLevel.Information, "Starting command: (.*)"),
            new LogEntry(LogLevel.Information, "Command finished. Duration: (.*)")
        );

        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task StartAsync_RunnerFails_ExceptionIsHandled()
    {
        const string errorMessage = "Test";
        _commandRunner.RunAsync(CancellationToken.None).ThrowsAsync(new Exception(errorMessage));

        var action = () => _sut.StartAsync(CancellationToken.None);
        
        await action.Should().ThrowAsync<Exception>().WithMessage(errorMessage);

        _hostApplicationLifeTime.Received(1).StopApplication();
        await _commandRunner.Received(1).RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Running command"),
            new LogEntry(LogLevel.Information, "Starting command: (.*)"),
            new LogEntry(LogLevel.Critical, "Unhandled exception [(]Command: (.*)[)]: Test"),
            new LogEntry(LogLevel.Information, "Command finished. Duration: (.*)")
        );
        
        await Verify(_ansiConsole.Output);
    }

    [Fact]
    public async Task StartAsync_RunnerFailsWithFlurl_ExceptionIsHandled()
    {
        const string errorMessage = "Test";
        _commandRunner.RunAsync(CancellationToken.None).ThrowsAsync(CreateFlurlHttpException(errorMessage));

        var action = () => _sut.StartAsync(CancellationToken.None);
        
        await action.Should().ThrowAsync<Exception>().WithMessage(errorMessage);

        _hostApplicationLifeTime.Received(1).StopApplication();
        await _commandRunner.Received(1).RunAsync(CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Running command"),
            new LogEntry(LogLevel.Information, "Starting command: (.*)"),
            new LogEntry(LogLevel.Critical, "Unhandled exception [(]Command: (.*)[)]: Test"),
            new LogEntry(LogLevel.Information, "Command finished. Duration: (.*)")
        );
        
        await Verify(_ansiConsole.Output);
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