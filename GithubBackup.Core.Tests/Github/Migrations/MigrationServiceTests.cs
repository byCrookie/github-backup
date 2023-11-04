using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using FluentAssertions;
using Flurl.Http;
using GithubBackup.Core.Github.Clients;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Utils;
using GithubBackup.TestUtils.Flurl;
using GithubBackup.TestUtils.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GithubBackup.Core.Tests.Github.Migrations;

public class MigrationServiceTests
{
    private readonly MigrationService _sut;

    private readonly IGithubApiClient _githubApiClient;
    private readonly IFileSystem _mockFileSystem;
    private readonly ILogger<MigrationService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly Environment.Environment _environment;

    public MigrationServiceTests()
    {
        _githubApiClient = Substitute.For<IGithubApiClient>();
        _logger = Substitute.For<ILogger<MigrationService>>();
        _mockFileSystem = new MockFileSystem();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _environment = new Environment.Environment(_mockFileSystem);

        _sut = new MigrationService(_mockFileSystem, _githubApiClient, _dateTimeProvider, _logger);
    }

    [Fact]
    public async Task GetMigrationAsync_HasMigration_Return()
    {
        const long id = 1;
        const MigrationState state = MigrationState.Exporting;
        var createdAt = DateTime.Now;

        var reponse = new MigrationReponse(id, state, createdAt).ToFlurlJsonResponse();

        _githubApiClient
            .GetAsync($"/user/migrations/{id}", null, Arg.Any<CancellationToken>())
            .Returns(reponse);

        var result = await _sut.GetMigrationAsync(id, CancellationToken.None);

        result.Should().BeEquivalentTo(new Migration(id, state, createdAt));
    }

    [Fact]
    public async Task GetMigrationsAsync_HasMigrations_Return()
    {
        const long id = 1;
        const long id2 = 2;
        const MigrationState state = MigrationState.Exporting;
        var createdAt = DateTime.Now;
        var ct = CancellationToken.None;

        _githubApiClient.ReceiveJsonPagedAsync(
            "/user/migrations",
            100,
            Arg.Any<Func<List<MigrationReponse>, List<MigrationReponse>>>(),
            Arg.Any<Action<IFlurlRequest>?>(),
            ct
        ).Returns(new List<MigrationReponse>
        {
            new(id, state, createdAt),
            new(id2, state, createdAt)
        });

        var result = await _sut.GetMigrationsAsync(CancellationToken.None);

        result.Should().BeEquivalentTo(new List<Migration>
        {
            new(id, state, createdAt),
            new(id2, state, createdAt)
        });
    }

    [Fact]
    public async Task StartMigrationAsync_Started_Return()
    {
        const long id = 1;
        const MigrationState state = MigrationState.Exporting;
        var createdAt = DateTime.Now;

        var reponse = new MigrationReponse(id, state, createdAt).ToFlurlJsonResponse();

        _githubApiClient
            .PostJsonAsync("/user/migrations",
                Arg.Is<MigrationRequest>(r => r.ExcludeAttachements
                                              && r.ExcludeMetadata
                                              && r.ExcludeGitData
                                              && r.OrgMetadataOnly
                                              && r.ExcludeOwnerProjects
                                              && r.ExcludeReleases
                                              && r.LockRepositories
                                              && r.Repositories.SequenceEqual(new[] { "Test1", "Test2" })),
                null,
                Arg.Any<CancellationToken>()
            )
            .Returns(reponse);

        var options = new StartMigrationOptions(
            new[] { "Test1", "Test2" },
            true,
            true,
            true,
            true,
            true,
            true,
            true
        );

        var result = await _sut.StartMigrationAsync(options, CancellationToken.None);

        result.Should().BeEquivalentTo(new Migration(id, state, createdAt));
    }

    [Fact]
    public async Task PollAndDownloadMigrationAsync_HasMigration_Return()
    {
        var onPollOutput = new StringBuilder();

        const long id = 1;

        var today = new DateTime(2000, 1, 1);

        _dateTimeProvider.Now.Returns(today);

        _githubApiClient
            .GetAsync($"/user/migrations/{id}", ct: Arg.Any<CancellationToken>())
            .Returns(new MigrationReponse(id, MigrationState.Exported, today).ToFlurlJsonResponse());

        var downloadPath = _environment.Root("Test");
        _mockFileSystem.Directory.CreateDirectory(downloadPath.FullName);

        var reponse1 = new MigrationReponse(id, MigrationState.Pending, _dateTimeProvider.Now).ToFlurlJsonResponse();
        var reponse2 = new MigrationReponse(id, MigrationState.Exporting, _dateTimeProvider.Now).ToFlurlJsonResponse();
        var reponse3 = new MigrationReponse(id, MigrationState.Exported, _dateTimeProvider.Now).ToFlurlJsonResponse();

        _githubApiClient
            .GetAsync($"/user/migrations/{id}", null, Arg.Any<CancellationToken>())
            .Returns(reponse1, reponse2, reponse3);

        _githubApiClient
            .DownloadFileAsync($"/user/migrations/{id}/archive", Arg.Any<string>(), Arg.Any<string>(), null,
                Arg.Any<CancellationToken>())
            .Returns(call => _mockFileSystem.Path.Combine(call.ArgAt<string>(1), call.ArgAt<string>(2)))
            .AndDoes(call => _mockFileSystem.File.Create(_mockFileSystem.Path.Combine(call.ArgAt<string>(1), call.ArgAt<string>(2))));

        var options = new DownloadMigrationOptions(
            id,
            downloadPath,
            null,
            false,
            TimeSpan.FromSeconds(1)
        );

        var result = await _sut.PollAndDownloadMigrationAsync(options, m =>
        {
            onPollOutput.AppendLine($"{m.State}");
            return Task.CompletedTask;
        }, CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "Polling migration 1"),
            new LogEntry(LogLevel.Debug, "Getting migration 1"),
            new LogEntry(LogLevel.Information, $"Migration {id} is Pending"),
            new LogEntry(LogLevel.Debug, "Getting migration 1"),
            new LogEntry(LogLevel.Information, $"Migration {id} is Exporting"),
            new LogEntry(LogLevel.Debug, "Getting migration 1"),
            new LogEntry(LogLevel.Information, $"Migration {id} is Exported"),
            new LogEntry(LogLevel.Debug, "Getting migration 1")
        );

        result.Should().BeEquivalentTo(_mockFileSystem.Path.Combine(downloadPath.FullName, $"{_dateTimeProvider.Now:yyyyMMddHHmmss}_migration_{id}.tar.gz"));

        onPollOutput.ToString().Trim().Should().Be(
            $"""
             {MigrationState.Pending}
             {MigrationState.Exporting}
             {MigrationState.Exported}
             """
        );
    }

    [Fact]
    public async Task DownloadMigrationAsync_Download_ReturnPath()
    {
        const long id = 1;

        var today = new DateTime(2000, 1, 1);

        _dateTimeProvider.Now.Returns(today);

        _githubApiClient
            .GetAsync($"/user/migrations/{id}", ct: Arg.Any<CancellationToken>())
            .Returns(new MigrationReponse(id, MigrationState.Exported, today).ToFlurlJsonResponse());

        var downloadPath = _environment.Root("Test");
        _mockFileSystem.Directory.CreateDirectory(downloadPath.FullName);

        _githubApiClient
            .DownloadFileAsync($"/user/migrations/{id}/archive", Arg.Any<string>(), Arg.Any<string>(), null,
                Arg.Any<CancellationToken>())
            .Returns(call => _mockFileSystem.Path.Combine(call.ArgAt<string>(1), call.ArgAt<string>(2)))
            .AndDoes(call => _mockFileSystem.File.Create(_mockFileSystem.Path.Combine(call.ArgAt<string>(1), call.ArgAt<string>(2))));

        var options = new DownloadMigrationOptions(
            id,
            downloadPath,
            null,
            false
        );

        var result = await _sut.DownloadMigrationAsync(options, CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "Getting migration 1")
        );

        result.Should().BeEquivalentTo(_mockFileSystem.Path.Combine(downloadPath.FullName, $"{_dateTimeProvider.Now:yyyyMMddHHmmss}_migration_{id}.tar.gz"));
    }

    [Fact]
    public async Task DownloadMigrationAsync_DownloadButMigrationExists_ThrowException()
    {
        const long id = 1;

        var today = new DateTime(2000, 1, 1);

        _dateTimeProvider.Now.Returns(today);

        _githubApiClient
            .GetAsync($"/user/migrations/{id}", ct: Arg.Any<CancellationToken>())
            .Returns(new MigrationReponse(id, MigrationState.Exported, today).ToFlurlJsonResponse());

        var downloadPath = _environment.Root("Test");
        _mockFileSystem.Directory.CreateDirectory(downloadPath.FullName);

        var downloadFile = $"{_dateTimeProvider.Now:yyyyMMddHHmmss}_migration_{id}.tar.gz";
        _mockFileSystem.File.Create(_environment.Root(downloadPath.FullName, downloadFile).FullName);

        _githubApiClient
            .DownloadFileAsync($"/user/migrations/{id}/archive", Arg.Any<string>(), Arg.Any<string>(), null,
                Arg.Any<CancellationToken>())
            .Returns(call => _mockFileSystem.Path.Combine(call.ArgAt<string>(1), call.ArgAt<string>(2)))
            .AndDoes(call => _mockFileSystem.File.Create(_mockFileSystem.Path.Combine(call.ArgAt<string>(1), call.ArgAt<string>(2))));

        var options = new DownloadMigrationOptions(
            id,
            downloadPath,
            null,
            false
        );

        var action = () => _sut.DownloadMigrationAsync(options, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "Getting migration 1")
        );
    }

    [Fact]
    public async Task DownloadMigrationAsync_DownloadAndOverride_DeleteBackupsWithSameId()
    {
        const long id = 1;

        var today = new DateTime(2000, 1, 1);

        _dateTimeProvider.Now.Returns(today);

        _githubApiClient
            .GetAsync($"/user/migrations/{id}", ct: Arg.Any<CancellationToken>())
            .Returns(new MigrationReponse(id, MigrationState.Exported, today).ToFlurlJsonResponse());

        var downloadPath = _environment.Root("Test");
        _mockFileSystem.Directory.CreateDirectory(downloadPath.FullName);

        var date10DaysAgo = new DateTime(2000, 12, 2);
        var date5DaysAgo = new DateTime(2000, 12, 7);
        var dateNow = new DateTime(2000, 12, 12);

        var backup1 = $"{date10DaysAgo:yyyyMMddHHmmss}_migration_{id}.tar.gz";
        var backup2 = $"{date5DaysAgo:yyyyMMddHHmmss}_migration_{id}.tar.gz";
        var backup3 = $"{dateNow:yyyyMMddHHmmss}_migration_{id + 1}.tar.gz";
        var backup1Path = _environment.Root(downloadPath.FullName, backup1).FullName;
        var backup2Path = _environment.Root(downloadPath.FullName, backup2).FullName;
        var backup3Path = _environment.Root(downloadPath.FullName, backup3).FullName;
        _mockFileSystem.File.Create(backup1Path);
        _mockFileSystem.File.Create(backup2Path);
        _mockFileSystem.File.Create(backup3Path);

        _githubApiClient
            .DownloadFileAsync($"/user/migrations/{id}/archive", Arg.Any<string>(), Arg.Any<string>(), null,
                Arg.Any<CancellationToken>())
            .Returns(call => _mockFileSystem.Path.Combine(call.ArgAt<string>(1), call.ArgAt<string>(2)))
            .AndDoes(call => _mockFileSystem.File.Create(_mockFileSystem.Path.Combine(call.ArgAt<string>(1), call.ArgAt<string>(2))));

        var options = new DownloadMigrationOptions(
            id,
            downloadPath
        );

        var result = await _sut.DownloadMigrationAsync(options, CancellationToken.None);

        _mockFileSystem.File.Exists(backup1Path).Should().BeFalse();
        _mockFileSystem.File.Exists(backup2Path).Should().BeFalse();
        _mockFileSystem.File.Exists(backup3Path).Should().BeTrue();

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "Getting migration 1"),
            new LogEntry(LogLevel.Information, "Overwriting backups"),
            new LogEntry(LogLevel.Information, "Deleting identical backup 20001202000000_migration_1.tar.gz"),
            new LogEntry(LogLevel.Information, "Deleting identical backup 20001207000000_migration_1.tar.gz")
        );

        result.Should().BeEquivalentTo(_mockFileSystem.Path.Combine(downloadPath.FullName, $"{_dateTimeProvider.Now:yyyyMMddHHmmss}_migration_{id}.tar.gz"));
    }

    [Fact]
    public async Task DownloadMigrationAsync_DownloadAndNumberOfArguments_DeleteBackupsWhenToMany()
    {
        const long id = 1;

        var today = new DateTime(2000, 1, 1);

        _dateTimeProvider.Now.Returns(today);

        _githubApiClient
            .GetAsync($"/user/migrations/{id}", ct: Arg.Any<CancellationToken>())
            .Returns(new MigrationReponse(id, MigrationState.Exported, today).ToFlurlJsonResponse());

        var downloadPath = _environment.Root("Test");
        _mockFileSystem.Directory.CreateDirectory(downloadPath.FullName);

        var date11DaysAgo = new DateTime(2000, 12, 1);
        var date10DaysAgo = new DateTime(2000, 12, 2);
        var date5DaysAgo = new DateTime(2000, 12, 7);
        var dateNow = new DateTime(2000, 12, 12);

        var backup0 = $"{date11DaysAgo:yyyyMMddHHmmss}_migration_{id + 2}.tar.gz";
        var backup1 = $"{date10DaysAgo:yyyyMMddHHmmss}_migration_{id}.tar.gz";
        var backup2 = $"{date5DaysAgo:yyyyMMddHHmmss}_migration_{id}.tar.gz";
        var backup3 = $"{dateNow:yyyyMMddHHmmss}_migration_{id + 1}.tar.gz";
        var backup0Path = _environment.Root(downloadPath.FullName, backup0).FullName;
        var backup1Path = _environment.Root(downloadPath.FullName, backup1).FullName;
        var backup2Path = _environment.Root(downloadPath.FullName, backup2).FullName;
        var backup3Path = _environment.Root(downloadPath.FullName, backup3).FullName;
        _mockFileSystem.File.Create(backup0Path);
        _mockFileSystem.File.Create(backup1Path);
        _mockFileSystem.File.Create(backup2Path);
        _mockFileSystem.File.Create(backup3Path);

        _githubApiClient
            .DownloadFileAsync($"/user/migrations/{id}/archive", Arg.Any<string>(), Arg.Any<string>(), null,
                Arg.Any<CancellationToken>())
            .Returns(call => _mockFileSystem.Path.Combine(call.ArgAt<string>(1), call.ArgAt<string>(2)))
            .AndDoes(call => _mockFileSystem.File.Create(_mockFileSystem.Path.Combine(call.ArgAt<string>(1), call.ArgAt<string>(2))));

        var options = new DownloadMigrationOptions(
            id,
            downloadPath,
            2,
            false
        );

        var result = await _sut.DownloadMigrationAsync(options, CancellationToken.None);

        _logger.VerifyLogs(
            new LogEntry(LogLevel.Debug, "Getting migration 1"),
            new LogEntry(LogLevel.Information, "Applying retention rules"),
            new LogEntry(LogLevel.Information,
                "Deleting backup 20001207000000_migration_1.tar.gz because to many backups are present"),
            new LogEntry(LogLevel.Information,
                "Deleting backup 20001202000000_migration_1.tar.gz because to many backups are present"),
            new LogEntry(LogLevel.Information,
                "Deleting backup 20001201000000_migration_3.tar.gz because to many backups are present")
        );

        _mockFileSystem.File.Exists(backup0Path).Should().BeFalse();
        _mockFileSystem.File.Exists(backup1Path).Should().BeFalse();
        _mockFileSystem.File.Exists(backup2Path).Should().BeFalse();
        _mockFileSystem.File.Exists(backup3Path).Should().BeTrue();

        result.Should().BeEquivalentTo(_mockFileSystem.Path.Combine(downloadPath.FullName, $"{_dateTimeProvider.Now:yyyyMMddHHmmss}_migration_{id}.tar.gz"));
    }
}