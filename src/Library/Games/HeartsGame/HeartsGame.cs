using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoolCardGames.Library.Games.HeartsGame;

// TODO: use events for all player rendering (dealing, people playing cards, etc)? or just an event to refresh + desc?

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
        public HeartsGame Make(List<IPlayerGameSession<HeartsCard>> playerSessions)
        {
            if (playerSessions.Count != NumPlayers)
                throw new ArgumentException($"{nameof(playerSessions)} must have {NumPlayers} elements, but it has {playerSessions.Count} elements");

            var gameState = new HeartsGameState();
            gameState.Players = new ReadOnlyCollection<HeartsPlayerState>(
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
        foreach (HeartsPlayerIntermediate playerIntermediate in playerIntermediates)
        {
            logger.LogInformation("Player at index {PlayerIndex} is {PlayerName} ({SessionType})",
                playerIntermediate.GameStatePlayerIndex,
                playerIntermediate.Session.PlayerDetails.Name,
                playerIntermediate.Session.SessionType);
        }

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
    public ReadOnlyCollection<TPlayerState> Players { get; set; } = ReadOnlyCollection<TPlayerState>.Empty;
}

public class PlayerState<TCard>
    where TCard : Card
{
    public Cards<TCard> Hand { get; set; } = [];
}

public record PlayerIntermediate<TCard, TPlayerState, TGameState>(
    IPlayerGameSession<TCard> Session,
    TGameState GameState,
    int GameStatePlayerIndex)
    where TCard : Card
    where TPlayerState : PlayerState<TCard>
    where TGameState : GameState<TCard, TPlayerState>
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

public record HeartsPlayerIntermediate(
    IPlayerGameSession<HeartsCard> Session,
    HeartsGameState GameState,
    int GameStatePlayerIndex)
    : PlayerIntermediate<HeartsCard, HeartsPlayerState, HeartsGameState>(Session, GameState, GameStatePlayerIndex);

////////////////////////////////////////////////////////////////////////////////

// TODO: prob make a Player feature folder w/in Core

public record PlayerDetails(string Name)
{
    public static readonly PlayerDetails AnonymousAnteater = new("Anonymous Anteater");
    public static readonly PlayerDetails AnonymousBat = new("Anonymous Bat");
    public static readonly PlayerDetails AnonymousChinchilla = new("Anonymous Chinchilla");
    public static readonly PlayerDetails AnonymousDog = new("Anonymous Dog");
    public static readonly PlayerDetails AnonymousElephant = new("Anonymous Elephant");
}

public interface IPlayerGameSession<TCard>
{
    PlayerDetails PlayerDetails { get; }
    string SessionType { get; }
}

public class CliPlayerGameSession<TCard>(PlayerDetails playerDetails) : IPlayerGameSession<TCard>
{
    public PlayerDetails PlayerDetails => playerDetails;
    public string SessionType => "Terminal";
}

public class GameAi<TCard>(PlayerDetails playerDetails) : IPlayerGameSession<TCard>
{
    public PlayerDetails PlayerDetails => playerDetails;
    public string SessionType => "Artificial";
}