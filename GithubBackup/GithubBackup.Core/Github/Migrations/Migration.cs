﻿namespace GithubBackup.Core.Github.Migrations;

public class Migration
{
    public long Id { get; }
    public MigrationState? State { get; }

    public Migration(long id, MigrationState? state)
    {
        Id = id;
        State = state;
    }
}