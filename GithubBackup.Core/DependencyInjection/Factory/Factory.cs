using Microsoft.Extensions.DependencyInjection;

namespace GithubBackup.Core.DependencyInjection.Factory;

internal sealed class Factory(IServiceProvider serviceProvider) : IFactory
{
    public T Create<T>()
        where T : notnull
    {
        if (serviceProvider.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException(
                $"Type {typeof(T).FullName} cannot be resolved. Register it explicitly."
            );
        }

        return service;
    }

    public object Create(Type type)
    {
        var service = serviceProvider.GetService(type);

        if (service is null)
        {
            throw new ArgumentException(
                $"Type {type.FullName} cannot be resolved. Register it explicitly."
            );
        }

        return service;
    }

    public T Create<T>(Type type)
        where T : notnull
    {
        if (serviceProvider.GetService(type) is not T service)
        {
            throw new ArgumentException(
                $"Type {typeof(T).FullName} cannot be resolved. Register it explicitly."
            );
        }

        return service;
    }
}

public class Factory<T>(IFactory factory) : IFactory<T>
    where T : notnull
{
    public T Create()
    {
        return factory.Create<T>();
    }
}

public class Factory<TParameter, T>(IServiceProvider serviceProvider) : IFactory<TParameter, T>
    where T : notnull
    where TParameter : notnull
{
    public T Create(TParameter parameter)
    {
        return ActivatorUtilities.CreateInstance<T>(serviceProvider, parameter);
    }
}

public class Factory<TParameter1, TParameter2, T>(IServiceProvider serviceProvider)
    : IFactory<TParameter1, TParameter2, T>
    where T : notnull
    where TParameter1 : notnull
    where TParameter2 : notnull
{
    public T Create(TParameter1 parameter1, TParameter2 parameter2)
    {
        return ActivatorUtilities.CreateInstance<T>(serviceProvider, parameter1, parameter2);
    }
}

public class Factory<TParameter1, TParameter2, TParameter3, T>(IServiceProvider serviceProvider)
    : IFactory<TParameter1, TParameter2, TParameter3, T>
    where T : notnull
    where TParameter1 : notnull
    where TParameter2 : notnull
    where TParameter3 : notnull
{
    public T Create(TParameter1 parameter1, TParameter2 parameter2, TParameter3 parameter3)
    {
        return ActivatorUtilities.CreateInstance<T>(
            serviceProvider,
            parameter1,
            parameter2,
            parameter3
        );
    }
}

public class Factory<TParameter1, TParameter2, TParameter3, TParameter4, T>(
    IServiceProvider serviceProvider
) : IFactory<TParameter1, TParameter2, TParameter3, TParameter4, T>
    where T : notnull
    where TParameter1 : notnull
    where TParameter2 : notnull
    where TParameter3 : notnull
    where TParameter4 : notnull
{
    public T Create(
        TParameter1 parameter1,
        TParameter2 parameter2,
        TParameter3 parameter3,
        TParameter4 parameter4
    )
    {
        return ActivatorUtilities.CreateInstance<T>(
            serviceProvider,
            parameter1,
            parameter2,
            parameter3,
            parameter4
        );
    }
}

public class Factory<TParameter1, TParameter2, TParameter3, TParameter4, TParameter5, T>(
    IServiceProvider serviceProvider
) : IFactory<TParameter1, TParameter2, TParameter3, TParameter4, TParameter5, T>
    where T : notnull
    where TParameter1 : notnull
    where TParameter2 : notnull
    where TParameter3 : notnull
    where TParameter4 : notnull
    where TParameter5 : notnull
{
    public T Create(
        TParameter1 parameter1,
        TParameter2 parameter2,
        TParameter3 parameter3,
        TParameter4 parameter4,
        TParameter5 parameter5
    )
    {
        return ActivatorUtilities.CreateInstance<T>(
            serviceProvider,
            parameter1,
            parameter2,
            parameter3,
            parameter4,
            parameter5
        );
    }
}
