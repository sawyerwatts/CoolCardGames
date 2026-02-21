using System.Threading.Channels;

using CoolCardGames.Library.Core.GameEventTypes;
using CoolCardGames.Library.Core.MiscUtils;

using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CoolCardGames.XUnitTests.Library.Core;

public class GameHarnessTests
{
    [Fact]
    public async Task TestThatChannelFanOutCompletesWhenGameCompletesNormally()
    {
        var game = Substitute.For<IGame>();
        game.Play(CancellationToken.None).Returns(Task.CompletedTask);

        var eventChannel = Channel.CreateUnbounded<GameEventEnvelope>();
        var channelFanOut = new ChannelFanOut<GameEventEnvelope>(eventChannel.Reader, NullLogger<ChannelFanOut<GameEventEnvelope>>.Instance);
        using (var harness = new GameHarness(game, eventChannel, channelFanOut, NullLogger<GameHarness>.Instance, []))
        {
            Assert.False(channelFanOut.Completed);
            await harness.Play(CancellationToken.None);
        }

        // It may take a bit for the disposal to complete.
        for (var i = 0; i < 5; i++)
        {
            if (channelFanOut.Completed)
            {
                break;
            }

            await Task.Delay(50);
        }

        Assert.True(channelFanOut.Completed);
    }

    [Fact]
    public async Task TestThatChannelFanOutCompletesWhenGameCrashes()
    {
        var game = Substitute.For<IGame>();
        game.Play(CancellationToken.None).ThrowsAsync(new InvalidOperationException("Uh oh"));

        var eventChannel = Channel.CreateUnbounded<GameEventEnvelope>();
        var channelFanOut = new ChannelFanOut<GameEventEnvelope>(eventChannel.Reader, NullLogger<ChannelFanOut<GameEventEnvelope>>.Instance);
        using (var harness = new GameHarness(game, eventChannel, channelFanOut, NullLogger<GameHarness>.Instance, []))
        {
            Assert.False(channelFanOut.Completed);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await harness.Play(CancellationToken.None));
        }

        // It may take a bit for the disposal to complete.
        for (var i = 0; i < 5; i++)
        {
            if (channelFanOut.Completed)
            {
                break;
            }

            await Task.Delay(50);
        }

        Assert.True(channelFanOut.Completed);
    }
}