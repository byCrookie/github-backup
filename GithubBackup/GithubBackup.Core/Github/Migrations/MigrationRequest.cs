﻿using System.Text.Json.Serialization;

namespace GithubBackup.Core.Github.Migrations;

internal class MigrationRequest
{
    [JsonPropertyName("repositories")]
    public IReadOnlyCollection<string> Repositories { get; }
    
    public MigrationRequest(IReadOnlyCollection<string> repositories)
    {
        Repositories = repositories;
    }
}