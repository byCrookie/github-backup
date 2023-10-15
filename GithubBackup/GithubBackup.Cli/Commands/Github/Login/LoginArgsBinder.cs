using System.CommandLine.Binding;

namespace GithubBackup.Cli.Commands.Github.Login;

internal sealed class LoginArgsBinder : BinderBase<LoginArgs>
{
    protected override LoginArgs GetBoundValue(BindingContext bindingContext)
    {
        var token = bindingContext.ParseResult.GetValueForOption(LoginArgs.TokenOption);
        var deviceFlowAuth = bindingContext.ParseResult.GetValueForOption(LoginArgs.DeviceFlowAuthOption);

        return new LoginArgs(
            token,
            deviceFlowAuth
        );
    }
}