using System.CommandLine;
using GithubBackup.Cli.Commands.Global;

namespace GithubBackup.Cli.Commands.Github.Migrations;

internal sealed class MigrationsArgs
{
    public bool Id { get; }

    public MigrationsArgs(bool id)
    {
        Id = id;
    }
}