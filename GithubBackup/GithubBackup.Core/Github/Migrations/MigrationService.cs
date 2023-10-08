using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Flurl.Http;
using GithubBackup.Core.Github.Flurl;
using GithubBackup.Core.Github.Repositories;

namespace GithubBackup.Core.Github.Migrations;

internal partial class MigrationService : IMigrationService
{
    private readonly IFileSystem _fileSystem;

    public MigrationService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
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

        if (_fileSystem.File.Exists(_fileSystem.Path.Combine(options.Destination.FullName, fileName)))
        {
            throw new Exception($"A backup with the id {options.Id} already exists.");
        }
        
        return $"/user/migrations/{options.Id}/archive"
            .DownloadFileGithubApiAsync(options.Destination.FullName, fileName);
    }

    private void ApplyRetentionRules(DownloadMigrationOptions options)
    {
        if (options.NumberOfBackups == 0)
        {
            throw new Exception("The number of backups cannot be 0.");
        }
        
        var backups = _fileSystem.Directory
            .GetFiles(options.Destination.FullName, "*", SearchOption.TopDirectoryOnly)
            .Select(file => BackupFileNameRegex().Match(file))
            .Where(match => match.Success)
            .ToList();

        if (backups.Count >= options.NumberOfBackups)
        {
            var backupsToDelete = backups
                .OrderByDescending(match => match.Groups["Date"].Value)
                .Skip(options.NumberOfBackups.Value - 1);

            foreach (var backup in backupsToDelete)
            {
                _fileSystem.File.Delete(_fileSystem.Path.Combine(options.Destination.FullName, backup.Value));
            }
        }
    }

    private void OverwriteBackups(DownloadMigrationOptions options)
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
                _fileSystem.File.Delete(_fileSystem.Path.Combine(options.Destination.FullName, backup.Value));
            }
        }
    }
    
    [GeneratedRegex(@"^(?<Date>\d{14})_migration_(?<Id>\d+)\.tar\.gz$", RegexOptions.Compiled)]
    private static partial Regex BackupFileNameRegex();
}