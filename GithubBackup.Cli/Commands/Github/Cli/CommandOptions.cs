using GithubBackup.Cli.Commands.Global;
using Microsoft.Extensions.Hosting;

namespace GithubBackup.Cli.Commands.Github.Cli;

public class CommandOptions
{
    public required GlobalArguments GlobalArguments { get; init; }
    public required Action<HostApplicationBuilder> AfterServices { get; init; }
}