﻿namespace GithubBackup.Core.Tests.DependencyInjection.Factory;

public interface ITestClass
{
    public ITestDependency1 TestDependency1 { get; }
    public ITestDependency2 TestDependency2 { get; }
}
