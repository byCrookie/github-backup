﻿using System.Text.RegularExpressions;
using Flurl;
using Flurl.Http;
using GithubBackup.Core.Github.Authentication;
using GithubBackup.Core.Github.Flurl;
using GithubBackup.Core.Github.Migrations;
using GithubBackup.Core.Github.Repositories;
using GithubBackup.Core.Github.Users;
using Microsoft.Extensions.Logging;
using Polly;

namespace GithubBackup.Core.Github;

internal partial class GithubService : IGithubService
{
    private readonly ILogger<GithubService> _logger;
    
    private const string ClientId = "e197b2a7e36e8a0d5ea9";

    public GithubService(ILogger<GithubService> logger)
    {
        _logger = logger;
    }
    
    public async Task<Migration> StartMigrationAsync(StartMigrationOptions options, CancellationToken ct)
    {
        var request = new MigrationRequest(options.Repositories);
        
        var response = await "/user/migrations"
            .PostJsonGithubApiAsync(request, ct)
            .ReceiveJson<MigrationReponse>();

        return new Migration(response.Id, response.Repositories.Select(r => new Repository(r.FullName)).ToList(), response.State, response.Url);
    }
    
    public async Task<List<Migration>> GetMigrationsAsync(CancellationToken ct)
    {
        var response = await "/user/migrations"
            .GetGithubApiAsync(ct)
            .ReceiveJson<List<MigrationReponse>>();

        return response.Select(m => new Migration(m.Id, m.Repositories.Select(r => new Repository(r.FullName)).ToList(), m.State, m.Url)).ToList();
    }
    
    public async Task<Migration> GetMigrationAsync(long id, CancellationToken ct)
    {
        var response = await $"/user/migrations/{id}"
            .GetGithubApiAsync(ct)
            .ReceiveJson<MigrationReponse>();

        return new Migration(response.Id, response.Repositories.Select(r => new Repository(r.FullName)).ToList(), response.State, response.Url);
    }

    public Task<string> DownloadMigrationAsync(DownloadMigrationOptions options, CancellationToken ct)
    {
        if (options.Overwrite)
        {
            OverwriteBackups(options);
        }

        if (options.NumberOfBackups is not null)
        {
            ApplyRetentionRules(options);
        }

        var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_migration_{options.Id}.tar.gz";

        if (File.Exists(Path.Combine(options.Destination.FullName, fileName)))
        {
            throw new Exception($"A backup with the id {options.Id} already exists.");
        }
        
        return $"/user/migrations/{options.Id}/archive"
            .DownloadFileGithubApiAsync(options.Destination.FullName, fileName);
    }

    private static void ApplyRetentionRules(DownloadMigrationOptions options)
    {
        var backups = Directory
            .GetFiles(options.Destination.FullName, "*", SearchOption.TopDirectoryOnly)
            .Select(file => BackupFileNameRegex().Match(file))
            .Where(match => match.Success)
            .ToList();

        if (backups.Count > options.NumberOfBackups)
        {
            var backupsToDelete = backups
                .OrderByDescending(match => match.Groups["Date"].Value)
                .Skip(options.NumberOfBackups.Value);

            foreach (var backup in backupsToDelete)
            {
                File.Delete(Path.Combine(options.Destination.FullName, backup.Value));
            }
        }
    }

    private static void OverwriteBackups(DownloadMigrationOptions options)
    {
        var identicalBackups = Directory
            .GetFiles(options.Destination.FullName, "*", SearchOption.TopDirectoryOnly)
            .Select(file => BackupFileNameRegex().Match(file))
            .Where(match => match.Success && match.Groups["Id"].Value == options.Id.ToString())
            .ToList();

        if (identicalBackups.Any())
        {
            foreach (var backup in identicalBackups)
            {
                File.Delete(Path.Combine(options.Destination.FullName, backup.Value));
            }
        }
    }

    public async Task<User> WhoAmIAsync(CancellationToken ct)
    {
        var response = await "/user"
            .GetGithubApiAsync(ct)
            .ReceiveJson<UserResponse>();

        return new User(response.Login, response.Name);
    }

    public async Task<DeviceAndUserCodes> RequestDeviceAndUserCodesAsync(CancellationToken ct)
    {
        const string scope = "repo user user:email read:user";

        var response = await "/login/device/code"
            .PostJsonGithubWebAsync(new { client_id = ClientId, scope }, ct)
            .ReceiveJson<DeviceAndUserCodesResponse>();

        return new DeviceAndUserCodes(
            response.DeviceCode,
            response.UserCode,
            response.VerificationUri,
            response.ExpiresIn,
            response.Interval
        );
    }

    public async Task<AccessToken> PollForAccessTokenAsync(string deviceCode, int interval, CancellationToken ct)
    {
        const string grantType = "urn:ietf:params:oauth:grant-type:device_code";

        var currentInterval = new IntervalWrapper(TimeSpan.FromSeconds(interval));

        var policy = Policy
            .HandleResult<AccessTokenResponse>(response => !string.IsNullOrWhiteSpace(response.Error))
            .RetryForeverAsync(response => OnRetryAsync(response.Result, currentInterval, ct));

        var response = await policy.ExecuteAsync(() => "/login/oauth/access_token"
            .PostJsonGithubWebAsync(new { client_id = ClientId, device_code = deviceCode, grant_type = grantType }, ct)
            .ReceiveJson<AccessTokenResponse>());

        return new AccessToken(response.AccessToken!, response.TokenType!, response.Scope!);
    }

    public async Task<IReadOnlyCollection<Repository>> GetRepositoriesAsync(CancellationToken ct)
    {
        var response = await "/user/repos"
            .SetQueryParam("affiliation", "owner")
            .GetJsonGithubApiPagedAsync<List<RepositoryResponse>, RepositoryResponse>(100, r => r, ct);

        return new List<Repository>(response.Select(r => new Repository(r.FullName)));
    }

    private async Task OnRetryAsync(AccessTokenResponse response, IntervalWrapper intervalWrapper, CancellationToken ct)
    {
        switch (response.Error)
        {
            case "authorization_pending":
            {
                var delay = intervalWrapper.Interval;
                _logger.LogInformation("Authorization pending. Retrying in {Seconds} seconds", delay.TotalSeconds);
                await Task.Delay(delay, ct);
                return;
            }
            case "slow_down":
            {
                var newDelay = TimeSpan.FromSeconds(response.Interval ?? intervalWrapper.Interval.TotalSeconds + 5);
                intervalWrapper.Update(newDelay);
                _logger.LogInformation("Slow down. Retrying in {Seconds} seconds", newDelay.TotalSeconds);
                await Task.Delay(newDelay, ct);
                return;
            }
            case "expired_token":
            {
                throw new Exception("The device code has expired.");
            }
            case "access_denied":
            {
                throw new Exception("The user has denied the request.");
            }
            default:
            {
                throw new Exception($"Unknown error: {response.Error} - {response.ErrorDescription} - {response.ErrorUri}");
            }
        }
    }

    [GeneratedRegex(@"^(?<Date>\d{14})_migration_(?<Id>\d+)\.tar\.gz$", RegexOptions.Compiled)]
    private static partial Regex BackupFileNameRegex();
}