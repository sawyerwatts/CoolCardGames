using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core;

/// <remarks>
/// If you want to actually code a game, you'll want to extend <see cref="Game{TPlayerState}"/>.
/// This interface is more for ease of proxying and testing and stuff.
/// </remarks>
public interface IGame : IDisposable
{
    string Name { get; }
    Task Play(CancellationToken cancellationToken);
    void PlayAndDisposeInBackgroundThread(CancellationToken cancellationToken);
}

public abstract class Game<TPlayerState>(
    IGameEventPublisher gameEventPublisher,
    GameState<TPlayerState> gameState,
    IReadOnlyList<IPlayer> players,
    ILogger<Game<TPlayerState>> logger)
    : IGame
    where TPlayerState : PlayerState
{
    public abstract string Name { get; }

    protected abstract object? SettingsToBeLogged { get; }

    public void PlayAndDisposeInBackgroundThread(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await Play(cancellationToken);
            }
            // Play() logs if an exception occurs so this doesn't need to.
            finally
            {
                try
                {
                    Dispose();
                }
                catch (Exception exc)
                {
                    logger.LogCritical(exc, "Game instance of {GameName} encountered an uncaught exception while disposing", Name);
                }
            }
        }, cancellationToken);
    }

    public async Task Play(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Beginning a game with settings {Settings}", SettingsToBeLogged);
            for (var i = 0; i < players.Count; i++)
                logger.LogInformation("Player at index {PlayerIndex} is {PlayerCard}", i, players[i].AccountCard);

            await gameEventPublisher.Publish(new GameEvent.GameStarted(Name), cancellationToken);
            await ActuallyPlay(cancellationToken);
            await gameEventPublisher.Publish(new GameEvent.GameEnded(Name, CompletedNormally: true), cancellationToken);
        }
        catch (Exception exc)
        {
            logger.LogCritical(exc, "A game crashed due to an uncaught exception");

            try
            {
                await gameEventPublisher.Publish(new GameEvent.GameEnded(Name, CompletedNormally: false), cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not publish the game completion event");
            }
        }
    }

    /// <remarks>
    /// Implementations should send <see cref="GameEvent"/>s via <see cref="IGameEventPublisher"/>.
    /// <br />
    /// The caller has a try/catch around this method, so implementations do not need a top-level
    /// try/catch.
    /// </remarks>
    protected abstract Task ActuallyPlay(CancellationToken cancellationToken);

    public abstract void Dispose();
}