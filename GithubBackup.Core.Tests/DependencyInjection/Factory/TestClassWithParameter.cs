namespace GithubBackup.Core.Tests.DependencyInjection.Factory;

public class TestClassWithParameter(
    string parameter,
    ITestDependency1 testDependency1,
    ITestDependency2 testDependency2
) : ITestClassWithParameter
{
    public string Parameter { get; } = parameter;
    public ITestDependency1 TestDependency1 { get; } = testDependency1;
    public ITestDependency2 TestDependency2 { get; } = testDependency2;
}
