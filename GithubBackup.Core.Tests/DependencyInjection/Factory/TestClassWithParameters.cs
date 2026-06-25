namespace GithubBackup.Core.Tests.DependencyInjection.Factory;

public class TestClassWithParameters(
    string parameter1,
    ITestDependency1 testDependency1,
    ITestDependency2 testDependency2,
    string parameter2
) : ITestClassWithParameters
{
    public string Parameter1 { get; } = parameter1;
    public ITestDependency1 TestDependency1 { get; } = testDependency1;
    public ITestDependency2 TestDependency2 { get; } = testDependency2;
    public string Parameter2 { get; } = parameter2;
}
