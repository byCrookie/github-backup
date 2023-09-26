using System.CommandLine;
using GitHubBackup.Cli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var destinationOption = new Option<DirectoryInfo>(
    aliases: new[] { "-d", "--destination" },
    getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory()),
    description: "The path to put the backup in");

var promptOption = new Option<bool>(
    aliases: new[] { "--prompt" },
    getDefaultValue: () => true,
    description: "Prompt before backup");

var rootCommand = new RootCommand("Github Backup");
rootCommand.AddOption(destinationOption);
rootCommand.AddOption(promptOption);

rootCommand.SetHandler((dest, prompt) => Run(() => Backup(dest, prompt)), destinationOption, promptOption);

return await rootCommand.InvokeAsync(args);

async Task Run(Func<Task> action)
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddHostedService(sp => new ActionService(sp.GetRequiredService<IHostApplicationLifetime>(), async () => await action()));
    var host = builder.Build();
    await host.RunAsync();
}

Task Backup(DirectoryInfo directoryInfo, bool prompt)
{
    if (prompt)
    {
        Console.WriteLine("Do you want to backup? (y/n)");
        if (!new List<ConsoleKey> { ConsoleKey.Y }.Contains(Console.ReadKey().Key))
        {
            return Task.CompletedTask;
        }
    }
    
    return Task.CompletedTask;
}