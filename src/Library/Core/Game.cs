using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core;

public abstract class Game
{
    private readonly IGameEventPublisher _gameEventPublisher;
    private readonly ILogger<Game> _logger;

    protected Game(IGameEventPublisher gameEventPublisher, ILogger<Game> logger)
    {
        _gameEventPublisher = gameEventPublisher;
        _logger = logger;
    }

    public abstract string Name { get; }

    public async Task<PlayResult> Play(CancellationToken cancellationToken)
    {
        try
        {
            await _gameEventPublisher.Publish(new GameEvent.GameStarted(Name), cancellationToken);
            await ActuallyPlay(cancellationToken);
            return new PlayResult();
        }
        catch (Exception exc)
        {
            _logger.LogCritical(exc, "A game crashed due to an unexpected exception");

            Exception? publishFailureException = null;
            try
            {
                await _gameEventPublisher.Publish(new GameEvent.GameEnded(Name, CompletedNormally: false), cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not publish the game completion event");
                publishFailureException = e;
            }

            return new PlayResult(exc, publishFailureException);
        }
    }

    public readonly record struct PlayResult(
        Exception? ExceptionWhenPlayingGame = null,
        Exception? ExceptionWhenFailedToPublishGameEventForFailure = null);

    /// <remarks>
    /// Implementations should send <see cref="GameEvent"/>s via <see cref="IGameEventPublisher"/>.
    /// <br />
    /// The caller has a try/catch around this method, so implementations do not need a top-level
    /// try/catch.
    /// </remarks>
    protected abstract Task ActuallyPlay(CancellationToken cancellationToken);
}