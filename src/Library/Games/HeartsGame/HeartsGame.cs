using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoolCardGames.Library.Games.HeartsGame;

// TODO: use events for all player rendering (dealing, people playing cards, etc)? or just an event to refresh + desc?

// TODO: how handle data visibility to diff players?

public class HeartsGame(
    IReadOnlyList<HeartsPlayer> players,
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
        foreach (HeartsPlayer player in players)
        {
            logger.LogInformation("Player at index {PlayerIndex} is {PlayerName}",
                player.GameStatePlayerIndex, player.DisplayName);
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

// TODO: write unit tests for these funcs
// TODO: update these funcs to pass additional, human-readable validation info
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

    private TPlayerState PlayerState => gameState.Players[gameStatePlayerIndex];
    private Cards<TCard> Hand => PlayerState.Hand;

    /// <remarks>
    /// This will take the selected card out of the appropriate <see cref="GameState{TCard,TPlayerState}.Players"/>' <see cref="PlayerState{TCard}.Hand"/>.
    /// </remarks>
    /// <param name="validateChosenCard">
    /// This will take the current player hand and the pre-validated in-range index of the card to
    /// play, and return true iff it is valid to play that card.
    /// </param>
    /// <param name="cancellationToken"></param>
    public async Task<TCard> PlayCard(Func<Cards<TCard>, int, bool> validateChosenCard, CancellationToken cancellationToken)
    {
        bool validCardToPlay = false;
        int iCardToPlay = -1;
        while (!validCardToPlay)
        {
            iCardToPlay = await session.PromptForIndexOfCardToPlay(Hand, cancellationToken);
            if (iCardToPlay < 0 || iCardToPlay >= Hand.Count)
                continue;

            validCardToPlay = validateChosenCard(Hand, iCardToPlay);
        }

        TCard cardToPlay = Hand[iCardToPlay];
        Hand.RemoveAt(iCardToPlay);
        return cardToPlay;
    }

    /// <remarks>
    /// This will take the selected card(s) out of the appropriate <see cref="GameState{TCard,TPlayerState}.Players"/>' <see cref="PlayerState{TCard}.Hand"/>.
    /// </remarks>
    /// <param name="validateChosenCards">
    /// This will take the current player hand and the pre-validated in-range and unique indexes of
    /// the cards to play, and return true iff it is valid to play those cards.
    /// </param>
    /// <param name="cancellationToken"></param>
    public async Task<Cards<TCard>> PlayCards(Func<Cards<TCard>, List<int>, bool> validateChosenCards, CancellationToken cancellationToken)
    {
        bool validCardsToPlay = false;
        List<int> iCardsToPlay = [];
        while (!validCardsToPlay)
        {
            iCardsToPlay = await session.PromptForIndexesOfCardsToPlay(Hand, cancellationToken);
            if (iCardsToPlay.Count != iCardsToPlay.Distinct().Count())
                continue;

            if (iCardsToPlay.Any(iCardToPlay => iCardToPlay < 0 || iCardToPlay >= Hand.Count))
                continue;

            validCardsToPlay = validateChosenCards(Hand, iCardsToPlay);
        }

        Cards<TCard> cardsToPlay = new(capacity: iCardsToPlay.Count);
        foreach (int iCardToPlay in iCardsToPlay.OrderDescending())
        {
            cardsToPlay.Add(Hand[iCardToPlay]);
            Hand.RemoveAt(iCardToPlay);
        }

        return cardsToPlay;
    }
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

// TODO: update these funcs to pass additional, human-readable validation info
public abstract class PlayerSession<TCard>(string displayName)
    where TCard : Card
{
    public string DisplayName => displayName;

    public abstract Task<int> PromptForIndexOfCardToPlay(Cards<TCard> cards, CancellationToken cancellationToken);

    public abstract Task<List<int>> PromptForIndexesOfCardsToPlay(Cards<TCard> cards, CancellationToken cancellationToken);
}

// TODO: move to Cli.csproj
// TODO: have a configurable delay b/w messages
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