namespace GithubBackup.Core.DependencyInjection.Factory;

public interface IFactory
{
    TService Create<TService>() where TService : notnull;
    object Create(Type type);
    TService Create<TService>(Type type) where TService : notnull;
}
    
public interface IFactory<out TService> where TService : notnull
{
    TService Create();
}
    
public interface IFactory<in TParameter, out TImplementation> where TImplementation : notnull
{
    TImplementation Create(TParameter parameter);
}
    
public interface IFactory<in TParameter1, in TParameter2, out TImplementation> where TImplementation : notnull
{
    TImplementation Create(TParameter1 parameter1, TParameter2 parameter2);
}
    
public interface IFactory<in TParameter1, in TParameter2, in TParameter3, out TImplementation> where TImplementation : notnull
{
    TImplementation Create(TParameter1 parameter1, TParameter2 parameter2, TParameter3 parameter3);
}
    
public interface IFactory<in TParameter1, in TParameter2, in TParameter3, in TParameter4, out TImplementation> where TImplementation : notnull
{
    TImplementation Create(TParameter1 parameter1, TParameter2 parameter2, TParameter3 parameter3, TParameter4 parameter4);
}
    
public interface IFactory<in TParameter1, in TParameter2, in TParameter3, in TParameter4, in TParameter5, out TImplementation> where TImplementation : notnull
{
    TImplementation Create(TParameter1 parameter1, TParameter2 parameter2, TParameter3 parameter3, TParameter4 parameter4, TParameter5 parameter5);
}