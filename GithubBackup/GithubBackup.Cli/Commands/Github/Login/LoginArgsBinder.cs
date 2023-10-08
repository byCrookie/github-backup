using System.CommandLine.Binding;
using GithubBackup.Cli.Utils;

namespace GithubBackup.Cli.Commands.Github.Login;

internal class LoginArgsBinder : BinderBase<LoginArgs>
{
    protected override LoginArgs GetBoundValue(BindingContext bindingContext)
    {
        var token = bindingContext.ParseResult.GetRequiredValueForOption(LoginArgs.TokenOption);
        var deviceFlowAuth = bindingContext.ParseResult.GetRequiredValueForOption(LoginArgs.DeviceFlowAuthOption);

        return new LoginArgs(
            token,
            deviceFlowAuth
        );
    }
}