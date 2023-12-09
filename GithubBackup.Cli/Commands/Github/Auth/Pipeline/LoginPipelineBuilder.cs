using GithubBackup.Core.DependencyInjection.Factory;

namespace GithubBackup.Cli.Commands.Github.Auth.Pipeline;

internal class LoginPipelineBuilder : ILoginPipelineBuilder
{
    private readonly IFactory<IDefaultPipeline> _defaultPipelineFactory;
    private readonly IFactory<ITokenFromConfigurationPipeline> _tokenFromConfigurationPipelineFactory;
    private readonly IFactory<ITokenArgPipeline> _tokenArgPipelineFactory;
    private readonly IFactory<IPersistedPipeline> _persistedPipelineFactory;
    private readonly IFactory<IDeviceFlowAuthPipeline> _deviceFlowAuthPipelineFactory;

    public LoginPipelineBuilder(
        IFactory<IDefaultPipeline> defaultPipelineFactory,
        IFactory<ITokenFromConfigurationPipeline> tokenFromConfigurationPipelineFactory,
        IFactory<ITokenArgPipeline> tokenArgPipelineFactory,
        IFactory<IPersistedPipeline> persistedPipelineFactory,
        IFactory<IDeviceFlowAuthPipeline> deviceFlowAuthPipelineFactory
    )
    {
        _defaultPipelineFactory = defaultPipelineFactory;
        _tokenFromConfigurationPipelineFactory = tokenFromConfigurationPipelineFactory;
        _tokenArgPipelineFactory = tokenArgPipelineFactory;
        _persistedPipelineFactory = persistedPipelineFactory;
        _deviceFlowAuthPipelineFactory = deviceFlowAuthPipelineFactory;
    }
    
    public ILoginPipeline PersistedOnly()
    {
        var persistedPipeline = _persistedPipelineFactory.Create();
        var defaultPipeline = _defaultPipelineFactory.Create();
        
        persistedPipeline.Next = defaultPipeline;
        return persistedPipeline;
    }

    public ILoginPipeline WithPersistent()
    {
        var persistedPipeline = _persistedPipelineFactory.Create();
        var defaultPipeline = _defaultPipelineFactory.Create();
        var tokenFromConfigurationPipeline = _tokenFromConfigurationPipelineFactory.Create();
        var tokenArgPipeline = _tokenArgPipelineFactory.Create();
        var deviceFlowAuthPipeline = _deviceFlowAuthPipelineFactory.Create();
        
        tokenArgPipeline.Next = tokenFromConfigurationPipeline;
        tokenFromConfigurationPipeline.Next = deviceFlowAuthPipeline;
        deviceFlowAuthPipeline.Next = persistedPipeline;
        persistedPipeline.Next = defaultPipeline;
        return persistedPipeline;
    }

    public ILoginPipeline WithoutPersistent()
    {
        var defaultPipeline = _defaultPipelineFactory.Create();
        var tokenFromConfigurationPipeline = _tokenFromConfigurationPipelineFactory.Create();
        var tokenArgPipeline = _tokenArgPipelineFactory.Create();
        var deviceFlowAuthPipeline = _deviceFlowAuthPipelineFactory.Create();
        
        tokenArgPipeline.Next = tokenFromConfigurationPipeline;
        tokenFromConfigurationPipeline.Next = deviceFlowAuthPipeline;
        deviceFlowAuthPipeline.Next = defaultPipeline;
        return tokenArgPipeline;
    }
}