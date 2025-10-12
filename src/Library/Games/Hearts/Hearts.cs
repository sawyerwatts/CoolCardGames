using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Games.Hearts;

// TODO: I almost want to log all game events

// TODO: the code is multi-braided: it does something, and it pushes a notification to do it
//       pass players to dealer so can notify?

// TODO: put iTrickStartPlayer into gameState?

// TODO: how handle data visibility to diff players?

// TODO: need to doc that each session and the game are all on diff threads
//       altho not necessarily

// TODO: doc that game events are used for UI changes in playersession

// TODO: decompose Hearts class to make it easier to test

/// <remarks>
/// It is intended to use <see cref="HeartsFactory"/> to instantiate this service.
/// </remarks>
public class Hearts(
    IReadOnlyList<HeartsPlayer> players,
    HeartsGameState gameState,
    IDealer dealer,
    HeartsSettings settings,
    ILogger<Hearts> logger)
    : IGame
{
    public const int NumPlayers = 4;

    public async Task Play(CancellationToken cancellationToken)
    {
        using var loggingScope = logger.BeginScope(
            "Hearts game with ID {GameId} and settings {Settings}",
            Guid.NewGuid(), settings);
        logger.LogInformation("Beginning a hearts game");
        foreach (HeartsPlayer player in players)
        {
            logger.LogInformation("Player at index {PlayerIndex} is {PlayerCard}",
                player.GameStatePlayerIndex, player.AccountCard);
        }

        CircularCounter dealerPosition = new(4, startAtEnd: true);
        while (gameState.Players.All(player => player.Score < settings.EndOfGamePoints))
        {
            await SetupRound((PassDirection)dealerPosition.Tick(), cancellationToken);

            int iTrickStartPlayer = gameState.Players.FindIndex(player =>
                player.Hand.Any(card => card.Value is TwoOfClubs));
            if (iTrickStartPlayer == -1)
                throw new InvalidOperationException(
                    $"Could not find a player with the {nameof(TwoOfClubs)}");

            iTrickStartPlayer = await PlayOutTrick(isFirstTrick: true, iTrickStartPlayer, cancellationToken);

            while (gameState.Players[0].Hand.Any())
                iTrickStartPlayer = await PlayOutTrick(isFirstTrick: false, iTrickStartPlayer, cancellationToken);

            if (gameState.Players.Any(player => player.Hand.Any()))
                throw new InvalidOperationException("Some players have cards left despite the 0th player having none");

            ScoreTricks();
        }

        // TODO: update this to use events: LogWinnersAndLosers();

        logger.LogInformation("Completed the hearts game");
    }

    private async Task SetupRound(PassDirection passDirection, CancellationToken cancellationToken)
    {
        gameState.IsHeartsBroken = false;

        logger.LogInformation("Shuffling, cutting, and dealing the deck to {NumPlayers}",
            NumPlayers);
        // TODO: could preserve and reshuffle cards instead of reinstantiating every round
        List<Cards<HeartsCard>> hands = dealer.ShuffleCutDeal(
            deck: HeartsCard.MakeDeck(Decks.Standard52()),
            numHands: NumPlayers);
        players.NotifyAll(Core.GameEvents.GameEvent.DeckShuffled.Singleton);
        players.NotifyAll(Core.GameEvents.GameEvent.DeckCut.Singleton);
        players.NotifyAll(new Core.GameEvents.GameEvent.DeckDealt(NumPlayers));

        for (int i = 0; i < NumPlayers; i++)
        {
            gameState.Players[i].Hand = hands[i];
            players.NotifyAll(new Core.GameEvents.GameEvent.HandGiven(players[i].AccountCard, hands[i].Count));
        }

        if (passDirection is PassDirection.Hold)
        {
            players.NotifyAll(GameEvent.HeartsHoldEmRound.Singleton);
            logger.LogInformation("Hold 'em round! No passing");
            return;
        }

        players.NotifyAll(new GameEvent.HeartsGetReadyToPass(passDirection));
        logger.LogInformation("Asking each player to select three cards to pass {PassDirection}",
            passDirection);
        List<Task<Cards<HeartsCard>>> takeCardsFromPlayerTasks = new(capacity: NumPlayers);
        for (int i = 0; i < NumPlayers; i++)
        {
            Task<Cards<HeartsCard>> task = players[i].PlayCards(
                validateChosenCards: (_, iCardsToPlay) => iCardsToPlay.Count == 3,
                cancellationToken);
            takeCardsFromPlayerTasks.Add(task);
        }

        await Task.WhenAll(takeCardsFromPlayerTasks).WaitAsync(cancellationToken);

        logger.LogInformation("Passing cards");
        for (int iSourcePlayer = 0; iSourcePlayer < NumPlayers; iSourcePlayer++)
        {
            CircularCounter sourcePlayerPosition = new(iSourcePlayer, NumPlayers);
            int iTargetPlayer = passDirection switch
            {
                PassDirection.Left => sourcePlayerPosition.CycleClockwise(updateInstance: false),
                PassDirection.Right => sourcePlayerPosition.CycleCounterClockwise(updateInstance: false),
                PassDirection.Across => sourcePlayerPosition.CycleClockwise(times: 2, updateInstance: false),
                _ => throw new UnreachableException(
                    $"Passing {passDirection} from {nameof(iSourcePlayer)} {iSourcePlayer}"),
            };

            Cards<HeartsCard> cardsToPass = takeCardsFromPlayerTasks[iSourcePlayer].Result;
            gameState.Players[iTargetPlayer].Hand.AddRange(cardsToPass);
        }

        players.NotifyAll(new GameEvent.HeartsCardsPassed(passDirection));
        logger.LogInformation("Hands are finalized");
    }

    private Task<int> PlayOutTrick(bool isFirstTrick, int iTrickStartPlayer, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private void ScoreTricks()
    {
        throw new NotImplementedException();
    }
}