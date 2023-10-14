using FluentAssertions;
using GithubBackup.Core.DependencyInjection;
using GithubBackup.Core.DependencyInjection.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.Tests.Integration.DependencyInjection.Factory;

public class FactorySingletonTests
{
    private readonly ServiceProvider _serviceProvider;

    public FactorySingletonTests()
    {
        var services = new ServiceCollection();
        services.AddDependencyInjection();
        services.AddSingleton<ITestClass, TestClass>();
        services.AddSingleton<ITestDependency1, TestDependency1>();
        services.AddSingleton<ITestDependency2, TestDependency2>();
        services.AddSingleton<ITestClassWithParameter, TestClassWithParameter>();
        services.AddSingleton<ITestClassWithParameters, TestClassWithParameters>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Create_WithoutGeneric_InjectDependencies()
    {
        var factory = _serviceProvider.GetRequiredService<IFactory>();
        var testClass = factory.Create<ITestClass>();
        testClass.TestDependency1.Should().NotBeNull();
        testClass.TestDependency2.Should().NotBeNull();
        
        var testClass2 = factory.Create<ITestClass>();
        testClass2.TestDependency1.Should().NotBeNull();
        testClass2.TestDependency2.Should().NotBeNull();
        
        testClass.Should().BeSameAs(testClass2);
    }

    [Fact]
    public void Create_WithGeneric_InjectDependencies()
    {
        var factory = _serviceProvider
            .GetRequiredService<IFactory<ITestClass>>();
        var testClass = factory.Create();
        testClass.TestDependency1.Should().NotBeNull();
        testClass.TestDependency2.Should().NotBeNull();
        
        var testClass2 = factory.Create();
        testClass2.TestDependency1.Should().NotBeNull();
        testClass2.TestDependency2.Should().NotBeNull();
        
        testClass.Should().BeSameAs(testClass2);
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
        
        var testClass2 = factory.Create(parameter);
        testClass2.TestDependency1.Should().NotBeNull();
        testClass2.TestDependency2.Should().NotBeNull();
        testClass2.Parameter.Should().Be(parameter);
        
        testClass.Should().NotBeSameAs(testClass2);
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
        
        var testClass2 = factory.Create(parameter1, parameter2);
        testClass2.TestDependency1.Should().NotBeNull();
        testClass2.TestDependency2.Should().NotBeNull();
        testClass2.Parameter1.Should().Be(parameter1);
        testClass2.Parameter2.Should().Be(parameter2);
        
        testClass.Should().NotBeSameAs(testClass2);
    }
}