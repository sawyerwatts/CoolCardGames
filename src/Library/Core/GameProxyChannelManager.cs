using System.Threading.Channels;

namespace CoolCardGames.Library.Core;

public sealed class GameProxyChannelManager(
    IGame game,
    Channel<GameEventEnvelope> eventChannel,
    ChannelFanOut<GameEventEnvelope> channelFanOut)
    : IGame
{
    public string Name => game.Name;

    public async Task<GamePlayResult> Play(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () => await channelFanOut.HandleFanOut(cancellationToken), cancellationToken);
        return await game.Play(cancellationToken);
    }

    public void Dispose()
    {
        eventChannel.Writer.Complete();
        game.Dispose();
    }
}