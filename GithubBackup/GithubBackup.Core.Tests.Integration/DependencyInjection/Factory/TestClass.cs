namespace GithubBackup.Core.Tests.Integration.DependencyInjection.Factory;

public class TestClass : ITestClass
{
    public ITestDependency1 TestDependency1 { get; }
    public ITestDependency2 TestDependency2 { get; }

    public TestClass(ITestDependency1 testDependency1, ITestDependency2 testDependency2)
    {
        TestDependency1 = testDependency1;
        TestDependency2 = testDependency2;
    }
}