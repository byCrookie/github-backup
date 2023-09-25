using System.CommandLine;

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

rootCommand.SetHandler(Backup, destinationOption, promptOption);

return await rootCommand.InvokeAsync(args);

void Backup(DirectoryInfo directoryInfo, bool prompt)
{
    if (prompt)
    {
        Console.WriteLine("Do you want to backup? (y/n)");
        if (!new List<ConsoleKey> { ConsoleKey.Y }.Contains(Console.ReadKey().Key))
        {
            return;
        }
    }
}