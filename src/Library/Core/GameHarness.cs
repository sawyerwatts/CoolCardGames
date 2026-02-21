using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core;

/// <summary>
/// This harness is a proxy of <see cref="IGame"/> to handle logistical concerns around starting
/// and cleaning up the game, log scoping/tracing, and other game-related but non-gameplay concerns
/// that occur alongside a game's execution.
/// </summary>
/// <remarks>
/// You could probably roll this into <see cref="Game{TCard,TPlayerState}"/>, but I believe that
/// would cause <see cref="Game{TCard,TPlayerState}"/> to have too many conceptual threads
/// (violating the Single Responsibility Principle, if you will).
/// </remarks>
/// <param name="game"></param>
/// <param name="eventChannel"></param>
/// <param name="channelFanOut"></param>
/// <param name="resourceCleanUpActions"></param>
public class GameHarness(
    IGame game,
    Channel<GameEventEnvelope> eventChannel,
    ChannelFanOut<GameEventEnvelope> channelFanOut,
    ILogger<GameHarness> logger,
    IEnumerable<Action> resourceCleanUpActions)
    : IGame
{
    public string Name => game.Name;

    public async Task Play(CancellationToken cancellationToken)
    {
        using var setup = Setup(cancellationToken);
        await game.Play(cancellationToken);
    }

    public void PlayAndDisposeInBackgroundThread(CancellationToken cancellationToken)
    {
        using var setup = Setup(cancellationToken);
        game.PlayAndDisposeInBackgroundThread(cancellationToken);
    }

    private IDisposable? Setup(CancellationToken cancellationToken)
    {
        var loggingScope = logger.BeginScope("{GameName} game with ID {GameId}", Name, Guid.NewGuid());
        _ = Task.Run(async () => await channelFanOut.HandleFanOut(cancellationToken), cancellationToken);
        return loggingScope;
    }

    public void Dispose()
    {
        logger.LogInformation("Disposing of an instance of game {GameName}", game.Name);
        eventChannel.Writer.Complete();
        foreach (Action resourceCleanUpAction in resourceCleanUpActions)
        {
            resourceCleanUpAction();
        }

        game.Dispose();
        GC.SuppressFinalize(this);
    }
}