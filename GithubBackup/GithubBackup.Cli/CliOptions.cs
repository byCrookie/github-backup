using GithubBackup.Cli.Console;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace GithubBackup.Cli;

public class CliOptions
{
    public ICliConsole? Console { get; init; } = new CliConsole(AnsiConsole.Console);
    public Action<HostApplicationBuilder> AfterConfiguration { get; init; } = _ => {};
    public Action<HostApplicationBuilder> AfterServices { get; init; } = _ => {};
}