using Microsoft.Extensions.Hosting;

namespace GithubBackup.Cli;

internal class RunOptions
{
    public required Action<HostApplicationBuilder> AfterConfiguration { get; init; }
    public required Action<HostApplicationBuilder> AfterServices { get; init; }
}