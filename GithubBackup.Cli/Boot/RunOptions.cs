using Microsoft.Extensions.Hosting;

namespace GithubBackup.Cli.Boot;

internal class RunOptions
{
    public required Action<HostApplicationBuilder> AfterServices { get; init; }
}