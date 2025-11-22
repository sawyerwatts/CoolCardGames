using System.Threading.Channels;

using CoolCardGames.Library.Core.GameEventTypes;
using CoolCardGames.Library.Core.MiscUtils;

using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

namespace CoolCardGames.XUnitTests.Library.Core;

public class GameProxyChannelManagerTests
{
    [Fact]
    public async Task Test()
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
                Assert.True(channelFanOut.Completed);
                break;
            }

            await Task.Delay(50);
        }
    }
}