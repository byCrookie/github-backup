namespace GithubBackup.Core.Tests.DependencyInjection.Factory;

public interface ITestClassWithParameters
{
    public string Parameter1 { get; }
    public ITestDependency1 TestDependency1 { get; }
    public ITestDependency2 TestDependency2 { get; }
    public string Parameter2 { get; }
}
