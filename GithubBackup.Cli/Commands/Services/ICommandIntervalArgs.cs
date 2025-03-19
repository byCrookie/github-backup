using GithubBackup.Cli.Commands.Interval;

namespace GithubBackup.Cli.Commands.Services;

internal interface ICommandIntervalArgs
{
    IntervalArgs IntervalArgs { get; }
}
