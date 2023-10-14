namespace GithubBackup.Core.Tests.Integration.DependencyInjection.Factory;

public class TestClassWithParameters : ITestClassWithParameters
{
    public string Parameter1 { get; }
    public ITestDependency1 TestDependency1 { get; }
    public ITestDependency2 TestDependency2 { get; }
    public string Parameter2 { get; }

    public TestClassWithParameters(string parameter1, ITestDependency1 testDependency1, ITestDependency2 testDependency2, string parameter2)
    {
        Parameter1 = parameter1;
        TestDependency1 = testDependency1;
        TestDependency2 = testDependency2;
        Parameter2 = parameter2;
    }
}