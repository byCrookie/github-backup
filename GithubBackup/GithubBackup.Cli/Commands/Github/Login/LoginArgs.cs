﻿using System.CommandLine;

namespace GithubBackup.Cli.Commands.Github.Login;

internal sealed class LoginArgs
{
    public string? Token { get; }
    public bool DeviceFlowAuth { get; }

    public LoginArgs(string? token, bool deviceFlowAuth)
    {
        Token = token;
        DeviceFlowAuth = deviceFlowAuth;
    }

    public static Option<string?> TokenOption { get; }
    public static Option<bool> DeviceFlowAuthOption { get; }

    static LoginArgs()
    {
        TokenOption = new Option<string?>(
            aliases: new[] { "-t", "--token" },
            getDefaultValue: () => null,
            description: LoginArgDescriptions.Token.Long
        ) { IsRequired = false };
        
        DeviceFlowAuthOption = new Option<bool>(
            aliases: new[] { "-dfa", "--device-flow-auth" },
            getDefaultValue: () => false,
            description: LoginArgDescriptions.DeviceFlowAuth.Long
        ) { IsRequired = false };
    }
    
    public static Option[] Options()
    {
        return new Option[]
        {
            TokenOption,
            DeviceFlowAuthOption
        };
    }
}