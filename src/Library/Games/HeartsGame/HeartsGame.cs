using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoolCardGames.Library.Games.HeartsGame;

// TODO: how handle data visibility to diff players?

public class HeartsGame(
    ReadOnlyCollection<HeartsPlayerIntermediate> playerIntermediates,
    HeartsGameState gameState,
    IDealer dealer,
    HeartsGame.Settings settings,
    ILogger<HeartsGame> logger)
    : IGame
{
    private const int NumPlayers = 4;

    public class Factory(
        IDealer dealer,
        IOptionsMonitor<Settings> settingsMonitor,
        ILogger<HeartsGame> logger)
    {
        public HeartsGame Make(List<IPlayerSession<HeartsCard>> playerSessions)
        {
            if (playerSessions.Count != NumPlayers)
                throw new ArgumentException($"{nameof(playerSessions)} must have {NumPlayers} elements, but it has {playerSessions.Count} elements");

            var gameState = new HeartsGameState();
            gameState.PlayerStates = new ReadOnlyCollection<HeartsPlayerState>(
                Enumerable.Range(0, NumPlayers)
                    .Select(_ => new HeartsPlayerState())
                    .ToList()
            );

            var playerIntermediates = playerSessions
                .Select((playerSession, i) => new HeartsPlayerIntermediate(playerSession, gameState, i))
                .ToList()
                .AsReadOnly();

            return new HeartsGame(playerIntermediates, gameState, dealer, settingsMonitor.CurrentValue, logger);
        }
    }

    public Task Play(CancellationToken cancellationToken)
    {
        using var loggingScope = logger.BeginScope("Beginning a new hearts game with game ID {GameId}", Guid.NewGuid());

        logger.LogInformation("Completed the hearts game");
        return Task.CompletedTask;
    }

    public class Settings
    {
        /// <summary>
        /// The game will end when a round is completed and someone has at least this many points.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int EndOfGamePoints { get; set; } = 100;
    }
}

////////////////////////////////////////////////////////////////////////////////

public class GameState<TCard, TPlayerState>
    where TCard : Card
    where TPlayerState : PlayerState<TCard>
{
    public ReadOnlyCollection<TPlayerState> PlayerStates { get; set; } = ReadOnlyCollection<TPlayerState>.Empty;
}

public class PlayerState<TCard>
    where TCard : Card
{
    public Cards<TCard> Hand { get; set; } = [];
}

public class PlayerIntermediate<TCard, TPlayerState>(
    IPlayerSession<TCard> playerSession,
    GameState<TCard, TPlayerState> gameState,
    int gameStatePlayerIndex)
    where TCard : Card
    where TPlayerState : PlayerState<TCard>
{
    // TODO: have methods to prompt for cards to play
}

////////////////////////////////////////////////////////////////////////////////

public class HeartsGameState : GameState<HeartsCard, HeartsPlayerState>;

public class HeartsPlayerState : PlayerState<HeartsCard>
{
    public List<Cards<HeartsCard>> TricksTaken { get; set; } = [];

    public int Score { get; set; } = 0;
}

public class HeartsPlayerIntermediate(
    IPlayerSession<HeartsCard> playerSession,
    HeartsGameState gameState,
    int gameStatePlayerIndex)
    : PlayerIntermediate<HeartsCard, HeartsPlayerState>(playerSession, gameState, gameStatePlayerIndex);

////////////////////////////////////////////////////////////////////////////////

public interface IPlayerSession<TCard>
{
}

public class CliPlayerSession<TCard> : IPlayerSession<TCard>
{
}

public class AiPlayerSession<TCard> : IPlayerSession<TCard>
{
}