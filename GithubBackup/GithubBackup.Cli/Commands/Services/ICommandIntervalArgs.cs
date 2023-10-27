using GithubBackup.Cli.Commands.Github.Interval;

namespace GithubBackup.Cli.Commands.Services;

internal interface ICommandIntervalArgs
{
    IntervalArgs IntervalArgs { get; }
}