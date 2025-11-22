using System.Threading.Channels;

using CoolCardGames.Library.Core.GameEventTypes;
using CoolCardGames.Library.Core.MiscUtils;

using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CoolCardGames.XUnitTests.Library.Core;

public class GameProxyChannelManagerTests
{
    [Fact]
    public async Task TestHappy()
    {
        var game = Substitute.For<IGame>();
        game.Play(CancellationToken.None).Returns(Task.FromResult(new GamePlayResult()));

        var eventChannel = Channel.CreateUnbounded<GameEventEnvelope>();
        var channelFanOut = new ChannelFanOut<GameEventEnvelope>(eventChannel.Reader, NullLogger<ChannelFanOut<GameEventEnvelope>>.Instance);
        using (var proxy = new GameProxyChannelManager(game, eventChannel, channelFanOut))
        {
            Assert.False(channelFanOut.Completed);
            await proxy.Play(CancellationToken.None);
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
    public async Task TestBad()
    {
        var game = Substitute.For<IGame>();
        game.Play(CancellationToken.None).ThrowsAsync(new InvalidOperationException("Uh oh"));

        var eventChannel = Channel.CreateUnbounded<GameEventEnvelope>();
        var channelFanOut = new ChannelFanOut<GameEventEnvelope>(eventChannel.Reader, NullLogger<ChannelFanOut<GameEventEnvelope>>.Instance);
        using (var proxy = new GameProxyChannelManager(game, eventChannel, channelFanOut))
        {
            Assert.False(channelFanOut.Completed);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await proxy.Play(CancellationToken.None));
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