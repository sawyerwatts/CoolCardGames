using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core;

public abstract class Game
{
    private event GameEventConsumer OnGameEvent = null!;

    private readonly ILogger<Game> _logger;

    protected Game(
        IEnumerable<GameEventConsumer> gameEventConsumers,
        ILogger<Game> logger)
    {
        _logger = logger;
        foreach (var gameEventConsumer in gameEventConsumers)
            OnGameEvent += gameEventConsumer;
        if (OnGameEvent is null)
            throw new ArgumentException($"{nameof(gameEventConsumers)} needs at least one element");
    }

    protected void PublishGameEvent(GameEvent gameEvent)
    {
        _logger.LogInformation("Publishing game event {GameEvent}", gameEvent);
        OnGameEvent.Invoke(gameEvent);
    }

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
            _logger.LogCritical(exc, "A game crashed due to an unexpected exception");
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
}