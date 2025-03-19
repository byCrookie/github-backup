using System.Runtime.Serialization;

namespace GithubBackup.Core.Tests.Flurl;

internal enum ItemEnum
{
    [EnumMember(Value = "test1")]
    Test1,

    [EnumMember(Value = "test2")]
    Test2,
}
