using System.Runtime.Serialization;

namespace GithubBackup.Core.Tests.Utils;

public enum TestEnum
{
    [EnumMember(Value = "test1")]
    Test1,
    Test2,
}
