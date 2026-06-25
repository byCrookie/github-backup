using System.IO.Abstractions;

namespace GithubBackup.Core.Environment;

public class Environment(IFileSystem fileSystem) : IEnvironment
{
    public int ExitCode { get; set; } = System.Environment.ExitCode;

    public IDirectoryInfo GetFolderPath(System.Environment.SpecialFolder folder)
    {
        return fileSystem.DirectoryInfo.New(System.Environment.GetFolderPath(folder));
    }

    public IDirectoryInfo Root(params string[] paths)
    {
        if (OperatingSystem.IsWindows())
            return fileSystem.DirectoryInfo.New(
                fileSystem.Path.Combine(new[] { @"C:\" }.Concat(paths).ToArray())
            );
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            return fileSystem.DirectoryInfo.New(
                fileSystem.Path.Combine(new[] { "/" }.Concat(paths).ToArray())
            );

        throw new NotSupportedException("Operating system not supported");
    }
}
