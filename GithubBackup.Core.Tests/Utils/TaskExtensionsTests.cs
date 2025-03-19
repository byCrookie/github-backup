using FluentAssertions;
using GithubBackup.Core.Utils;

namespace GithubBackup.Core.Tests.Utils;

public class TaskExtensionsTests
{
    [Fact]
    public async Task BoolOrCanceledAsFalseAsync_WhenIsBool_ThenReturnBool()
    {
        var task = new ValueTask<bool>(true);

        var result = await task.BoolOrCanceledAsFalseAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task BoolOrCanceledAsFalseAsync_WhenIsCanceled_ThenReturnFalse()
    {
        var task = ValueTask.FromCanceled<bool>(new CancellationToken(true));

        var result = await task.BoolOrCanceledAsFalseAsync();

        result.Should().BeFalse();
    }
}
