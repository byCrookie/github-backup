using System.CommandLine;
using GitHubBackup.Cli;
using GitHubBackup.Cli.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var quietOption = new Option<bool>(
    aliases: new[] { "-q", "--quiet" },
    getDefaultValue: () => false,
    description: "Do not print logs to console"
);

var logFileOption = new Option<FileInfo?>(
    aliases: new[] { "-l", "--log-file" },
    getDefaultValue: () => null,
    description: "The path to the log file"
);

var verbosityOption = new Option<LogLevel>(
    aliases: new[] { "-v", "--verbosity" },
    getDefaultValue: () => LogLevel.Warning,
    description: "The verbosity of the logs"
);

var interactiveOption = new Option<bool>(
    aliases: new[] { "-i", "--interactive" },
    getDefaultValue: () => false,
    description: "Select backup customizations interactively"
);

var destinationOption = new Option<DirectoryInfo>(
    aliases: new[] { "-d", "--destination" },
    getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory()),
    description: "The path to put the backup in"
) { IsRequired = true };

var rootCommand = new RootCommand("Github Backup");
rootCommand.AddGlobalOption(verbosityOption);
rootCommand.AddGlobalOption(quietOption);
rootCommand.AddGlobalOption(logFileOption);
rootCommand.AddGlobalOption(interactiveOption);

rootCommand.AddOption(destinationOption);

rootCommand.SetHandler(
    (verbosity, quiet, logFile, interactive, dest) =>
        Run(verbosity, quiet, logFile, () => Backup(dest, interactive)),
    verbosityOption,
    quietOption,
    logFileOption,
    interactiveOption,
    destinationOption
);

return await rootCommand.InvokeAsync(args);

async Task Run(LogLevel verbosity, bool quiet, FileSystemInfo? logFile, Func<Task> action)
{
    Log.Logger = CreateLoggerConfiguration(verbosity, logFile, quiet).CreateLogger();
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddHostedService(sp =>
        new ActionService(sp.GetRequiredService<IHostApplicationLifetime>(), async () => await action()));
    builder.Services.AddSerilog();
    var host = builder.Build();
    await host.RunAsync();
}

Task Backup(DirectoryInfo destination, bool interactive)
{
    if (interactive)
    {
        Console.WriteLine("Do you want to backup? (y/n)");
        if (!new List<ConsoleKey> { ConsoleKey.Y }.Contains(Console.ReadKey().Key))
        {
            return Task.CompletedTask;
        }
    }

    return Task.CompletedTask;
}

LoggerConfiguration CreateLoggerConfiguration(LogLevel logLevel, FileSystemInfo? fileSystemInfo, bool quiet)
{
    var configuration = new LoggerConfiguration()
        .MinimumLevel.Is(logLevel.MicrosoftToSerilogLevel());

    if (fileSystemInfo is not null)
    {
        configuration.WriteTo.File(
            fileSystemInfo.FullName,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 100_000_000,
            retainedFileCountLimit: 10,
            rollingInterval: RollingInterval.Infinite
        );
    }

    if (!quiet)
    {
        configuration.WriteTo.Console();
    }

    return configuration;
}