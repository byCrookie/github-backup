using Microsoft.Extensions.Hosting;

namespace GithubBackup.Cli.Boot;

internal class RunOptions
{
    public required TextWriter Output { get; init; }
    public required TextWriter Error { get; init; }
    public required Action<HostApplicationBuilder> AfterServices { get; init; }
}
