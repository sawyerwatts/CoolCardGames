using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core;

public abstract class Game(GameEventHandler gameEventHandler, ILogger<Game> logger)
{
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

    /// <summary>
    /// This will log the game event and then handle it.
    /// </summary>
    /// <param name="gameEvent"></param>
    protected void PublishGameEvent(GameEvent gameEvent)
    {
        logger.LogInformation("Publishing game event {GameEvent}", gameEvent);
        gameEventHandler(gameEvent);
    }
}