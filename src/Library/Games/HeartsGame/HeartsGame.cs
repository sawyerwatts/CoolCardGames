using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoolCardGames.Library.Games.HeartsGame;

// TODO: use events for all player rendering (dealing, people playing cards, etc)? or just an event to refresh + desc?

// TODO: how handle data visibility to diff players?

public class HeartsGame(
    IReadOnlyList<HeartsPlayer> playerIntermediates,
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
        public HeartsGame Make(List<PlayerSession<HeartsCard>> playerSessions)
        {
            if (playerSessions.Count != NumPlayers)
                throw new ArgumentException($"{nameof(playerSessions)} must have {NumPlayers} elements, but it has {playerSessions.Count} elements");

            var gameState = new HeartsGameState();
            gameState.Players = Enumerable.Range(0, NumPlayers)
                .Select(_ => new HeartsPlayerState())
                .ToList()
                .AsReadOnly();

            var playerIntermediates = playerSessions
                .Select((playerSession, i) => new HeartsPlayer(playerSession, gameState, i))
                .ToList()
                .AsReadOnly();

            return new HeartsGame(playerIntermediates, gameState, dealer, settingsMonitor.CurrentValue, logger);
        }
    }

    public Task Play(CancellationToken cancellationToken)
    {
        using var loggingScope = logger.BeginScope("Beginning a new hearts game with game ID {GameId}", Guid.NewGuid());
        foreach (HeartsPlayer playerIntermediate in playerIntermediates)
        {
            logger.LogInformation("Player at index {PlayerIndex} is {PlayerName}",
                playerIntermediate.GameStatePlayerIndex, playerIntermediate.DisplayName);
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
    public IReadOnlyList<TPlayerState> Players { get; set; } = new List<TPlayerState>().AsReadOnly();
}

public class PlayerState<TCard>
    where TCard : Card
{
    public Cards<TCard> Hand { get; set; } = [];
}

public class Player<TCard, TPlayerState, TGameState>(
    PlayerSession<TCard> session,
    TGameState gameState,
    int gameStatePlayerIndex)
    where TCard : Card
    where TPlayerState : PlayerState<TCard>
    where TGameState : GameState<TCard, TPlayerState>
{
    public string DisplayName => session.DisplayName;
    public int GameStatePlayerIndex => gameStatePlayerIndex;

    // TODO: have methods to prompt for cards to play
}

////////////////////////////////////////////////////////////////////////////////

public class HeartsGameState : GameState<HeartsCard, HeartsPlayerState>;

public class HeartsPlayerState : PlayerState<HeartsCard>
{
    public List<Cards<HeartsCard>> TricksTaken { get; set; } = [];

    public int Score { get; set; } = 0;
}

public class HeartsPlayer(
    PlayerSession<HeartsCard> session,
    HeartsGameState gameState,
    int gameStatePlayerIndex)
    : Player<HeartsCard, HeartsPlayerState, HeartsGameState>(session, gameState, gameStatePlayerIndex);

////////////////////////////////////////////////////////////////////////////////

public abstract class PlayerSession<TCard>(string displayName)
    where TCard : Card
{
    public string DisplayName => displayName;

    public abstract Task<int> PromptForIndexOfCardToPlay(Cards<TCard> cards, CancellationToken cancellationToken);

    public abstract Task<List<int>> PromptForIndexesOfCardsToPlay(Cards<TCard> cards, CancellationToken cancellationToken);
}

public class CliPlayerSession<TCard>(string displayName) : PlayerSession<TCard>(displayName)
    where TCard : Card
{
    public override Task<int> PromptForIndexOfCardToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task<List<int>> PromptForIndexesOfCardsToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class Ai<TCard>(string displayName) : PlayerSession<TCard>(displayName)
    where TCard : Card
{
    public override Task<int> PromptForIndexOfCardToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task<List<int>> PromptForIndexesOfCardsToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}