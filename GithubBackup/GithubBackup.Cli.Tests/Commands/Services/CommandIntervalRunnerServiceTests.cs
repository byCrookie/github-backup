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
public class CommandIntervalRunnerServiceTests
{
    private readonly CommandIntervalRunnerService _sut;
    private readonly ILogger<CommandIntervalRunnerService> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifeTime;
    private readonly ICommandRunner _commandRunner;
    private readonly IEnvironment _environment;
    private readonly TestConsole _ansiConsole;

    public CommandIntervalRunnerServiceTests()
    {
        _logger = Substitute.For<ILogger<CommandIntervalRunnerService>>();
        _hostApplicationLifeTime = Substitute.For<IHostApplicationLifetime>();
        _commandRunner = Substitute.For<ICommandRunner>();
        _environment = Substitute.For<IEnvironment>();
        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _ansiConsole = new TestConsole();
        var stopwatch = Substitute.For<IStopwatch>();

        var fakeStopWatch = new Stopwatch();
        stopwatch.StartNew().Returns(fakeStopWatch);

        _sut = new CommandIntervalRunnerService(
            new GlobalArgs(LogLevel.Debug, false, new FileInfo("test")),
            TimeSpan.FromSeconds(2), 
            _logger,
            _hostApplicationLifeTime,
            _commandRunner,
            _ansiConsole,
            dateTimeProvider,
            stopwatch
        );
    }

    [Fact]
    public async Task StartAsync_Runner_IsRunAndClosed()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        await _sut.StartAsync(cts.Token);

        _hostApplicationLifeTime.Received(1).StopApplication();
        await _commandRunner.Received(1).RunAsync(cts.Token);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Running command. Interval: 00:00:02"),
            new LogEntry(LogLevel.Information, "Starting command: (.*)"),
            new LogEntry(LogLevel.Information, "Command finished. Duration: (.*)"),
            new LogEntry(LogLevel.Information, "Waiting until 01.01.0001 00:00:02(.*) for next run")
        );

        await Verify(_ansiConsole.Output);

        _environment.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task StartAsync_RunnerFails_ExceptionIsHandled()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        const string errorMessage = "Test";
        _commandRunner.RunAsync(cts.Token).ThrowsAsync(new Exception(errorMessage));

        await _sut.StartAsync(cts.Token);

        _hostApplicationLifeTime.Received(1).StopApplication();
        await _commandRunner.Received(1).RunAsync(cts.Token);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Running command. Interval: 00:00:02"),
            new LogEntry(LogLevel.Information, "Starting command: (.*)"),
            new LogEntry(LogLevel.Error, """Unhandled exception \(Command: (.*)\): Test"""),
            new LogEntry(LogLevel.Information, "Command finished. Duration: 00:00:00"),
            new LogEntry(LogLevel.Information, "Waiting until 01.01.0001 00:00:02(.*) for next run")
        );
        
        await Verify(_ansiConsole.Output);

        _environment.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task StartAsync_RunnerFailsWithFlurl_ExceptionIsHandled()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        const string errorMessage = "Test";
        _commandRunner.RunAsync(cts.Token).ThrowsAsync(CreateFlurlHttpException(errorMessage));

        await _sut.StartAsync(cts.Token);

        _hostApplicationLifeTime.Received(1).StopApplication();
        await _commandRunner.Received(1).RunAsync(cts.Token);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Information, "Running command. Interval: 00:00:02"),
            new LogEntry(LogLevel.Information, "Starting command: (.*)"),
            new LogEntry(LogLevel.Error, """Unhandled exception \(Command: (.*)\): Test"""),
            new LogEntry(LogLevel.Information, "Command finished. Duration: 00:00:00"),
            new LogEntry(LogLevel.Information, "Waiting until 01.01.0001 00:00:02(.*) for next run")
        );
        
        await Verify(_ansiConsole.Output);

        _environment.ExitCode.Should().Be(0);
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