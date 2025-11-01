using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core;

public abstract class Game(ChannelWriter<GameEvent> gameEventWriter, ILogger<Game> logger)
{
    public abstract string Name { get; }

    /// <remarks>
    /// This method will never throw an exception, it will return it instead.
    /// </remarks>
    public async Task<Exception?> Play(CancellationToken cancellationToken)
    {
        try
        {
            await ActuallyPlay(cancellationToken);
            return null;
        }
        catch (Exception exc)
        {
            logger.LogCritical(exc, "A game crashed due to an unexpected exception");
            return exc;
        }
    }

    /// <remarks>
    /// Implementations should send <see cref="GameEvent"/>s via <see cref="PublishGameEvent"/>.
    /// <br />
    /// The caller has a try/catch around this method, so implementations do not need a top-level
    /// try/catch.
    /// </remarks>
    protected abstract Task ActuallyPlay(CancellationToken cancellationToken);

    protected async Task PublishGameEvent(GameEvent gameEvent, CancellationToken cancellationToken)
    {
        await gameEventWriter.WriteAsync(gameEvent, cancellationToken);
    }
}