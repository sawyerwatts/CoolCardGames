using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.GameEventUtils;

// TODO: Then HeartsGame can send that ID to Prompter to forward to Player which can can ensure the UI is up to date
// TODO: Impl (+doc?) the functionality to make sure UI is up to date when getting prompted

/// <summary>
/// Instead of having games and the dealer and other services writing directly to a channel, this
/// interface decouples event publishers and the event publishing. This is done primarily to allow
/// for easy enrichment of game events (without having to muddle the oh so reusable and modular
/// <see cref="ChannelFanOut{TMessage}"/>).
/// </summary>
public interface IGameEventPublisher
{
    ValueTask<GameEventEnvelope> Publish(GameEvent gameEvent, CancellationToken cancellationToken);
}

/// <remarks>
/// This is intended to be instantiated via <see cref="ChannelGameEventPublisherFactory"/> and to be
/// instantiated once per game instance.
/// </remarks>
public class ChannelGameEventPublisher(
    ChannelWriter<GameEventEnvelope> writer,
    ILogger<ChannelGameEventPublisher> logger)
    : IGameEventPublisher
{
    private uint _id = 0;

    public async ValueTask<GameEventEnvelope> Publish(GameEvent gameEvent, CancellationToken cancellationToken)
    {
        var currId = Interlocked.Increment(ref _id);
        var envelope = new GameEventEnvelope(gameEvent, currId.ToString());
        logger.LogInformation("Publishing {Envelope}", envelope);
        await writer.WriteAsync(envelope, cancellationToken);
        return envelope;
    }
}