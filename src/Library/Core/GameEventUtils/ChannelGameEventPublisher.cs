using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.GameEventUtils;

/// <summary>
/// Instead of having games and the dealer and other services writing directly to a channel, this
/// interface decouples event publishers and the event publishing. This is done primarily to allow
/// for easy enrichment of game events (without having to muddle the oh so reusable and modular
/// <see cref="ChannelFanOut{TMessage}"/>).
/// </summary>
public interface IGameEventPublisher
{
    ValueTask Publish(GameEvent gameEvent, CancellationToken cancellationToken);
}

/// <remarks>
/// This is intended to be instantiated via <see cref="ChannelGameEventPublisherFactory"/> and to be
/// instantiated once per game instance.
/// </remarks>
public class ChannelGameEventPublisher(
    ChannelWriter<GameEvent> writer,
    ILogger<ChannelGameEventPublisher> logger)
    : IGameEventPublisher
{
    private uint _id = 0;

    public ValueTask Publish(GameEvent gameEvent, CancellationToken cancellationToken)
    {
        var currId = Interlocked.Increment(ref _id);
        logger.LogInformation("Setting the game event's ID to {GameEventId}", currId);
        gameEvent = gameEvent with { Id = currId.ToString() };
        return writer.WriteAsync(gameEvent, cancellationToken);
    }
}