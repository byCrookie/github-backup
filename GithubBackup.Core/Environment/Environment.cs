using System.IO.Abstractions;

namespace GithubBackup.Core.Environment;

public class Environment : IEnvironment
{
    private readonly IFileSystem _fileSystem;

    public Environment(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public int ExitCode { get; set; } = System.Environment.ExitCode;
    
    public IDirectoryInfo GetFolderPath(System.Environment.SpecialFolder folder)
    {
        return _fileSystem.DirectoryInfo.New(System.Environment.GetFolderPath(folder));
    }
    
    public IDirectoryInfo Root(params string[] paths)
    {
        if (OperatingSystem.IsWindows())
            return _fileSystem.DirectoryInfo.New(_fileSystem.Path.Combine(new []{@"C:\"}.Concat(paths).ToArray()));
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            return _fileSystem.DirectoryInfo.New(_fileSystem.Path.Combine(new [] { "/" }.Concat(paths).ToArray()));
        
        throw new NotSupportedException("Operating system not supported");
    }
}