using System.IO.Abstractions;

namespace GithubBackup.Core.Environment;

public interface IEnvironment
{
    int ExitCode { get; set; }
    public IDirectoryInfo GetFolderPath(System.Environment.SpecialFolder folder);
    IDirectoryInfo Root(params string[] paths);
}