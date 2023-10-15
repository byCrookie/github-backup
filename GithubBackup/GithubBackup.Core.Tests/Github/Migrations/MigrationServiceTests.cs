﻿using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using FluentAssertions;
using Flurl.Http;
using GithubBackup.Core.Github.Clients;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Tests.Utils;
using GithubBackup.Core.Utils;
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

    public MigrationServiceTests()
    {
        _githubApiClient = Substitute.For<IGithubApiClient>();
        _logger = Substitute.For<ILogger<MigrationService>>();
        _mockFileSystem = new MockFileSystem();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();

        _sut = new MigrationService(_mockFileSystem, _githubApiClient, _dateTimeProvider, _logger);
    }

    [Fact]
    public async Task GetMigrationAsync_HasMigration_Return()
    {
        const long id = 1;
        const MigrationState state = MigrationState.Exporting;
        var createdAt = DateTime.Now;

        var reponse = new MigrationReponse(id, state, createdAt).ToFlurlResponse();

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

        var reponse = new MigrationReponse(id, state, createdAt).ToFlurlResponse();

        _githubApiClient
            .PostJsonAsync("/user/migrations",
                Arg.Is<MigrationRequest>(r => r.ExcludeAttachements
                                              && r.ExcludeMetadata
                                              && r.ExcludeGitData
                                              && r.ExcludeMetadataOnly
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
        var createdAt = DateTime.Now;

        var reponse1 = new MigrationReponse(id, MigrationState.Pending, createdAt).ToFlurlResponse();
        var reponse2 = new MigrationReponse(id, MigrationState.Exporting, createdAt).ToFlurlResponse();
        var reponse3 = new MigrationReponse(id, MigrationState.Exported, createdAt).ToFlurlResponse();

        var downloadPath = _mockFileSystem.DirectoryInfo.New(@"C:\Test");

        _githubApiClient
            .GetAsync($"/user/migrations/{id}", null, Arg.Any<CancellationToken>())
            .Returns(reponse1, reponse2, reponse3);

        const string downloadFile = @"C:\Test\Test.zip";

        _githubApiClient
            .DownloadFileAsync($"/user/migrations/{id}/archive", downloadPath.FullName, Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(downloadFile);

        var options = new DownloadMigrationOptions(
            id,
            downloadPath,
            null,
            false
        );

        var result = await _sut.PollAndDownloadMigrationAsync(options, m =>
        {
            onPollOutput.AppendLine($"{m.State}");
            return Task.CompletedTask;
        }, CancellationToken.None);

        _logger.VerifyLogs(
            (LogLevel.Debug, $"Migration {id} is Pending"),
            (LogLevel.Debug, $"Migration {id} is Exporting"),
            (LogLevel.Debug, $"Migration {id} is Exported")
        );

        result.Should().BeEquivalentTo(downloadFile);

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

        var downloadPath = _mockFileSystem.DirectoryInfo.New(@"C:\Test");

        const string downloadFile = @"C:\Test\Test.zip";

        _githubApiClient
            .DownloadFileAsync($"/user/migrations/{id}/archive", downloadPath.FullName, Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(downloadFile);

        var options = new DownloadMigrationOptions(
            id,
            downloadPath,
            null,
            false
        );

        var result = await _sut.DownloadMigrationAsync(options, CancellationToken.None);

        _logger.VerifyLogs();

        result.Should().BeEquivalentTo(downloadFile);
    }

    [Fact]
    public async Task DownloadMigrationAsync_DownloadButMigrationExists_ThrowException()
    {
        const long id = 1;

        var downloadPath = _mockFileSystem.DirectoryInfo.New(@"C:\Test");

        _dateTimeProvider.Now.Returns(new DateTime(2000, 1, 1));

        var downloadFile = $@"C:\Test\{_dateTimeProvider.Now:yyyyMMddHHmmss}_migration_{id}.tar.gz";

        _mockFileSystem.Directory.CreateDirectory(downloadPath.FullName);
        _mockFileSystem.File.Create(downloadFile);

        _githubApiClient
            .DownloadFileAsync($"/user/migrations/{id}/archive", downloadPath.FullName, Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(downloadFile);

        var options = new DownloadMigrationOptions(
            id,
            downloadPath,
            null,
            false
        );

        var action = () => _sut.DownloadMigrationAsync(options, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();
        _logger.VerifyLogs();
    }
    
    [Fact]
    public async Task DownloadMigrationAsync_DownloadAndOverride_DeleteBackupsWithSameId()
    {
        const long id = 1;
        
        var downloadPath = _mockFileSystem.DirectoryInfo.New(@"C:\Test");
        
        _mockFileSystem.Directory.CreateDirectory(downloadPath.FullName);
        
        var date10DaysAgo = new DateTime(2000, 12, 2);
        var date5DaysAgo = new DateTime(2000, 12, 7);
        var dateNow = new DateTime(2000, 12, 12);
        
        var backup1 = $@"C:\Test\{date10DaysAgo:yyyyMMddHHmmss}_migration_{id}.tar.gz";
        var backup2 = $@"C:\Test\{date5DaysAgo:yyyyMMddHHmmss}_migration_{id}.tar.gz";
        var backup3 = $@"C:\Test\{dateNow:yyyyMMddHHmmss}_migration_{id + 1}.tar.gz";
        _mockFileSystem.File.Create(backup1);
        _mockFileSystem.File.Create(backup2);
        _mockFileSystem.File.Create(backup3);
        
        const string downloadFile = @"C:\Test\Test.zip";

        _githubApiClient
            .DownloadFileAsync($"/user/migrations/{id}/archive", downloadPath.FullName, Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(downloadFile);

        var options = new DownloadMigrationOptions(
            id,
            downloadPath
        );

        var result = await _sut.DownloadMigrationAsync(options, CancellationToken.None);

        _mockFileSystem.File.Exists(backup1).Should().BeFalse();
        _mockFileSystem.File.Exists(backup2).Should().BeFalse();
        _mockFileSystem.File.Exists(backup3).Should().BeTrue();

        _logger.VerifyLogs(
            (LogLevel.Debug, "Deleting identical backup 20001202000000_migration_1.tar.gz"),
            (LogLevel.Debug, "Deleting identical backup 20001207000000_migration_1.tar.gz")
        );
        
        result.Should().BeEquivalentTo(downloadFile);
    }
    
    [Fact]
    public async Task DownloadMigrationAsync_DownloadAndNumberOfArguments_DeleteBackupsWhenToMany()
    {
        const long id = 1;
        
        var downloadPath = _mockFileSystem.DirectoryInfo.New(@"C:\Test");
        
        _mockFileSystem.Directory.CreateDirectory(downloadPath.FullName);
        
        var date11DaysAgo = new DateTime(2000, 12, 1);
        var date10DaysAgo = new DateTime(2000, 12, 2);
        var date5DaysAgo = new DateTime(2000, 12, 7);
        var dateNow = new DateTime(2000, 12, 12);
        
        var backup0 = $@"C:\Test\{date11DaysAgo:yyyyMMddHHmmss}_migration_{id + 2}.tar.gz";
        var backup1 = $@"C:\Test\{date10DaysAgo:yyyyMMddHHmmss}_migration_{id}.tar.gz";
        var backup2 = $@"C:\Test\{date5DaysAgo:yyyyMMddHHmmss}_migration_{id}.tar.gz";
        var backup3 = $@"C:\Test\{dateNow:yyyyMMddHHmmss}_migration_{id + 1}.tar.gz";
        _mockFileSystem.File.Create(backup0);
        _mockFileSystem.File.Create(backup1);
        _mockFileSystem.File.Create(backup2);
        _mockFileSystem.File.Create(backup3);
        
        const string downloadFile = @"C:\Test\Test.zip";

        _githubApiClient
            .DownloadFileAsync($"/user/migrations/{id}/archive", downloadPath.FullName, Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(downloadFile);

        var options = new DownloadMigrationOptions(
            id,
            downloadPath,
            2,
            false
        );

        var result = await _sut.DownloadMigrationAsync(options, CancellationToken.None);
        
        _logger.VerifyLogs(
            (LogLevel.Debug, "Deleting backup 20001207000000_migration_1.tar.gz because to many backups are present"),
            (LogLevel.Debug, "Deleting backup 20001202000000_migration_1.tar.gz because to many backups are present"),
            (LogLevel.Debug, "Deleting backup 20001201000000_migration_3.tar.gz because to many backups are present")
        );

        _mockFileSystem.File.Exists(backup0).Should().BeFalse();
        _mockFileSystem.File.Exists(backup1).Should().BeFalse();
        _mockFileSystem.File.Exists(backup2).Should().BeFalse();
        _mockFileSystem.File.Exists(backup3).Should().BeTrue();
        
        result.Should().BeEquivalentTo(downloadFile);
    }
}