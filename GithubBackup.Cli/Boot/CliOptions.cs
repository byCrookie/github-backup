using GithubBackup.Cli.Console;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace GithubBackup.Cli.Boot;

public class CliOptions
{
    public ICliConsole? Console { get; init; } = new CliConsole(AnsiConsole.Console);
    public Action<HostApplicationBuilder> AfterServices { get; init; } = _ => {};
}