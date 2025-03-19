namespace GithubBackup.Core.Tests.DependencyInjection.Factory;

public class TestClassWithParameter : ITestClassWithParameter
{
    public string Parameter { get; }
    public ITestDependency1 TestDependency1 { get; }
    public ITestDependency2 TestDependency2 { get; }

    public TestClassWithParameter(
        string parameter,
        ITestDependency1 testDependency1,
        ITestDependency2 testDependency2
    )
    {
        Parameter = parameter;
        TestDependency1 = testDependency1;
        TestDependency2 = testDependency2;
    }
}
