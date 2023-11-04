using FluentAssertions;
using GithubBackup.Core.DependencyInjection;
using GithubBackup.Core.DependencyInjection.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Tests.DependencyInjection.Factory;

public class FactoryTests
{
    private readonly ServiceProvider _serviceProvider;

    public FactoryTests()
    {
        var services = new ServiceCollection();
        services.AddDependencyInjection();
        services.AddTransient<ITestClass, TestClass>();
        services.AddTransient<ITestDependency1, TestDependency1>();
        services.AddTransient<ITestDependency2, TestDependency2>();
        services.AddTransient<ITestClassWithParameter, TestClassWithParameter>();
        services.AddTransient<ITestClassWithParameters, TestClassWithParameters>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Create_WithoutGeneric_InjectDependencies()
    {
        var factory = _serviceProvider.GetRequiredService<IFactory>();
        var testClass = factory.Create<ITestClass>();
        testClass.TestDependency1.Should().NotBeNull();
        testClass.TestDependency2.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithGeneric_InjectDependencies()
    {
        var factory = _serviceProvider
            .GetRequiredService<IFactory<ITestClass>>();
        var testClass = factory.Create();
        testClass.TestDependency1.Should().NotBeNull();
        testClass.TestDependency2.Should().NotBeNull();
    }
    
    [Fact]
    public void Create_WithGenericAndParameter_InjectDependencies()
    {
        const string parameter = "test";
        var factory = _serviceProvider
            .GetRequiredService<IFactory<string, TestClassWithParameter>>();
        var testClass = factory.Create(parameter);
        testClass.TestDependency1.Should().NotBeNull();
        testClass.TestDependency2.Should().NotBeNull();
        testClass.Parameter.Should().Be(parameter);
    }
    
    [Fact]
    public void Create_WithGenericAndParameters_InjectDependencies()
    {
        const string parameter1 = "test1";
        const string parameter2 = "test2";
        var factory = _serviceProvider
            .GetRequiredService<IFactory<string, string, TestClassWithParameters>>();
        var testClass = factory.Create(parameter1, parameter2);
        testClass.TestDependency1.Should().NotBeNull();
        testClass.TestDependency2.Should().NotBeNull();
        testClass.Parameter1.Should().Be(parameter1);
        testClass.Parameter2.Should().Be(parameter2);
    }
}