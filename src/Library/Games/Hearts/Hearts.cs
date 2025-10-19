using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Games.Hearts;

// TODO: the code is multi-braided: it does something, and it pushes a notification to do it

// TODO: review all logs and see if they can/should be events

// TODO: put iTrickStartPlayer into gameState?

// TODO: how handle data visibility to diff players?

// TODO: decompose Hearts class to make it easier to test

/// <remarks>
/// It is intended to use <see cref="HeartsFactory"/> to instantiate this service.
/// </remarks>
public class Hearts(
    GameEventHandler eventHandler,
    IReadOnlyList<HeartsPlayer> players,
    HeartsGameState gameState,
    IDealer dealer,
    HeartsSettings settings,
    ILogger<Hearts> logger)
    : Game(eventHandler, logger)
{
    public const int NumPlayers = 4;

    protected override async Task ActuallyPlay(CancellationToken cancellationToken)
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

        WinnersAndLosers();
        logger.LogInformation("Completed the hearts game");
    }

    private async Task SetupRound(PassDirection passDirection, CancellationToken cancellationToken)
    {
        gameState.IsHeartsBroken = false;
        HandleGameEvent(GameEvent.SettingUpNewRound.Singleton);

        logger.LogInformation("Shuffling, cutting, and dealing the deck to {NumPlayers}",
            NumPlayers);
        // TODO: could preserve and reshuffle cards instead of reinstantiating every round
        List<Cards<HeartsCard>> hands = dealer.ShuffleCutDeal(
            deck: HeartsCard.MakeDeck(Decks.Standard52()),
            numHands: NumPlayers);

        for (int i = 0; i < NumPlayers; i++)
        {
            gameState.Players[i].Hand = hands[i];
            HandleGameEvent(new GameEvent.HandGiven(players[i].AccountCard, hands[i].Count));
        }

        if (passDirection is PassDirection.Hold)
        {
            HandleGameEvent(HeartsGameEvent.HoldEmRound.Singleton);
            return;
        }

        HandleGameEvent(new HeartsGameEvent.GetReadyToPass(passDirection));
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

        HandleGameEvent(new HeartsGameEvent.CardsPassed(passDirection));
        HandleGameEvent(GameEvent.BeginningNewRound.Singleton);
    }

    private Task<int> PlayOutTrick(bool isFirstTrick, int iTrickStartPlayer, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private void ScoreTricks()
    {
        HandleGameEvent(GameEvent.ScoringRound.Singleton);
        List<int> roundScores = new(capacity: NumPlayers);
        foreach (HeartsPlayerState playerState in gameState.Players)
        {
            int roundScore = playerState.TricksTaken.Sum(trickCards => trickCards.Sum(card => card.Points));
            roundScores.Add(roundScore);
        }

        if (roundScores.Count(score => score == 0) == 3)
        {
            int iPlayerShotTheMoon = roundScores.FindIndex(score => score != 0);
            HandleGameEvent(new HeartsGameEvent.ShotTheMoon(players[iPlayerShotTheMoon].AccountCard));
            const int totalPointsInDeck = 26;
            for (int i = 0; i < roundScores.Count; i++)
            {
                if (i != iPlayerShotTheMoon)
                    roundScores[i] = totalPointsInDeck;
            }
        }

        for (int i = 0; i < roundScores.Count; i++)
        {
            gameState.Players[i].Score += roundScores[i];
            HandleGameEvent(new HeartsGameEvent.TrickScored(players[i].AccountCard, roundScores[i], gameState.Players[i].Score));
        }
    }

    private void WinnersAndLosers()
    {
        for (int i = 0; i < gameState.Players.Count; i++)
        {
            HeartsPlayerState playerState = gameState.Players[i];
            if (playerState.Score < settings.EndOfGamePoints)
                continue;
            logger.LogInformation("{AccountCard} is at or over {EndOfGamePoints} points with {TotalPoints}",
                players[i].AccountCard, settings.EndOfGamePoints, playerState.Score);
        }

        int minScore = gameState.Players.Min(player => player.Score);
        for (int i = 0; i < gameState.Players.Count; i++)
        {
            HeartsPlayerState playerState = gameState.Players[i];
            if (playerState.Score != minScore)
            {
                HandleGameEvent(new GameEvent.Loser(players[i].AccountCard));
                continue;
            }
            logger.LogInformation("{AccountCard} is the winner with {TotalPoints}", players[i].AccountCard, playerState.Score);
            HandleGameEvent(new GameEvent.Winner(players[i].AccountCard));
        }
    }
}