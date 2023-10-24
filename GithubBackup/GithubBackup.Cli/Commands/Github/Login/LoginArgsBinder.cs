using System.CommandLine.Binding;

namespace GithubBackup.Cli.Commands.Github.Login;

internal sealed class LoginArgsBinder : BinderBase<LoginArgs>
{
    private readonly LoginArguments _loginArguments;

    public LoginArgsBinder(LoginArguments loginArguments)
    {
        _loginArguments = loginArguments;
    }
    
    protected override LoginArgs GetBoundValue(BindingContext bindingContext)
    {
        var token = bindingContext.ParseResult.GetValueForOption(_loginArguments.TokenOption);
        var deviceFlowAuth = bindingContext.ParseResult.GetValueForOption(_loginArguments.DeviceFlowAuthOption);

        return new LoginArgs(
            token,
            deviceFlowAuth
        );
    }
}