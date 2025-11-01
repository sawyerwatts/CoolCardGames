using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Games.Hearts;

/* TODO: impl Bridge pattern (see diagram in readme)
   - make abstract Player
   - make PlayerBridge
   - rm HeartsPlayer
   - mv CliUser to CliPlayer
   - mv AiUse to AiPlayer
   - add healthcheck to Player so bridge can swap as needed
   - update xmldocs
 */

// TODO: replace GameEventHandler w/ Channel

// TODO: add an event (w/ ID) to say that game is about to ask player P for a card or cards, and
//       then send that ID in the request to P so P can make sure it's up to date
//       (also prob send game state so it can hard reset if needed?)

// TODO: mv log scoping to base class? would need a settings base class too

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

    public override string Name => "Hearts";

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

            gameState.IndexTrickStartPlayer = gameState.Players.FindIndex(player =>
                player.Hand.Any(card => card.Value is TwoOfClubs));
            if (gameState.IndexTrickStartPlayer == -1)
                throw new InvalidOperationException($"Could not find a player with the {nameof(TwoOfClubs)}");

            gameState.IsFirstTrick = true;
            while (gameState.Players[0].Hand.Any())
            {
                await PlayOutTrick(cancellationToken);
                gameState.IsFirstTrick = false;
            }

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
        PublishGameEvent(GameEvent.SettingUpNewRound.Singleton);

        logger.LogInformation("Shuffling, cutting, and dealing the deck to {NumPlayers}",
            NumPlayers);
        // TODO: could preserve and reshuffle cards instead of reinstantiating every round
        List<Cards<HeartsCard>> hands = dealer.ShuffleCutDeal(
            deck: HeartsCard.MakeDeck(Decks.Standard52()),
            numHands: NumPlayers);

        for (int i = 0; i < NumPlayers; i++)
        {
            gameState.Players[i].Hand = hands[i];
            PublishGameEvent(new GameEvent.HandGiven(players[i].AccountCard, hands[i].Count));
        }

        if (passDirection is PassDirection.Hold)
        {
            PublishGameEvent(HeartsGameEvent.HoldEmRound.Singleton);
            return;
        }

        PublishGameEvent(new HeartsGameEvent.GetReadyToPass(passDirection));
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

        PublishGameEvent(new HeartsGameEvent.CardsPassed(passDirection));
        PublishGameEvent(GameEvent.BeginningNewRound.Singleton);
    }

    // TODO: update and review this method w/ events in mind
    private async Task PlayOutTrick(CancellationToken cancellationToken)
    {
        CircularCounter iTrickPlayer = new(seed: gameState.IndexTrickStartPlayer, maxExclusive: NumPlayers);
        while (true) // TODO: at risk of infinite loop
        {
            logger.LogInformation("Getting trick's opening card from {AccountCard}", players[iTrickPlayer.N].AccountCard);
            if (!gameState.IsHeartsBroken && gameState.Players[gameState.IndexTrickStartPlayer].Hand.All(card => card.Value.Suit is Suit.Hearts))
            {
                logger.LogInformation(
                    "Hearts has not been broken and {AccountCard} only has hearts, skipping to the next player",
                    players[iTrickPlayer.N].AccountCard);
                iTrickPlayer.CycleClockwise();
            }
            else
                break;
        }

        HeartsCard openingCard = await players[gameState.IndexTrickStartPlayer].PlayCard(
            validateChosenCard: (hand, iCardToPlay) => gameState.IsFirstTrick
                ? hand[iCardToPlay].Value is TwoOfClubs
                : gameState.IsHeartsBroken || hand[iCardToPlay].Value.Suit is not Suit.Hearts,
            cancellationToken);
        logger.LogInformation("{AccountCard} played {CardValue}", players[iTrickPlayer.N].AccountCard, openingCard.Value);
        Cards<HeartsCard> trick = new(capacity: NumPlayers) { openingCard };
        Suit suitToFollow = openingCard.Value.Suit;

        while (iTrickPlayer.CycleClockwise() != gameState.IndexTrickStartPlayer)
        {
            var playerWithAction = players[iTrickPlayer.N];
            logger.LogInformation("Getting trick's next card from {AccountCard}", playerWithAction.AccountCard);
            HeartsCard chosenCard = await players[iTrickPlayer.N].PlayCard(
                validateChosenCard: (hand, iCardToPlay) =>
                {
                    if (!CheckPlayedCard.IsSuitFollowedIfPossible(suitToFollow, hand, iCardToPlay))
                        return false;
                    if (gameState.IsFirstTrick && hand[iCardToPlay].Points != 0)
                        return false;
                    return true;
                },
                cancellationToken);
            trick.Add(chosenCard);
            logger.LogInformation("{AccountCard} played {CardValue}", playerWithAction.AccountCard, chosenCard.Value);

            if (!gameState.IsHeartsBroken && chosenCard.Value.Suit is Suit.Hearts)
            {
                PublishGameEvent(new HeartsGameEvent.HeartsHaveBeenBroken(playerWithAction.AccountCard, chosenCard));
                gameState.IsHeartsBroken = true;
            }
        }

        if (trick.Count != NumPlayers)
            throw new InvalidOperationException($"After playing a trick, the trick has {trick.Count} cards but expected {NumPlayers} cards");

        IEnumerable<HeartsCard> onSuitCards = trick.Where(card => card.Value.Suit == suitToFollow);
        Rank highestOnSuitRank = GetHighest.Of(HeartsRankPriorities.Value, onSuitCards.Select(card => card.Value.Rank).ToList());
        int iTrickTakerOffsetFromStartPlayer = trick.FindIndex(card => card.Value.Suit == suitToFollow && card.Value.Rank == highestOnSuitRank);
        if (iTrickTakerOffsetFromStartPlayer == -1)
            throw new InvalidOperationException($"Could not find a card in the trick with suit {suitToFollow} and rank {highestOnSuitRank}");
        int iNextTrickStartPlayer = new CircularCounter(seed: gameState.IndexTrickStartPlayer, maxExclusive: NumPlayers)
            .Tick(delta: iTrickTakerOffsetFromStartPlayer);
        logger.LogInformation("{AccountCard} took the trick with {Card}", players[iNextTrickStartPlayer].AccountCard, trick[iTrickTakerOffsetFromStartPlayer]);
        gameState.Players[iNextTrickStartPlayer].TricksTaken.Add(trick);
        gameState.IndexTrickStartPlayer = iNextTrickStartPlayer;
    }

    private void ScoreTricks()
    {
        PublishGameEvent(GameEvent.ScoringRound.Singleton);
        List<int> roundScores = new(capacity: NumPlayers);
        foreach (HeartsPlayerState playerState in gameState.Players)
        {
            int roundScore = playerState.TricksTaken.Sum(trickCards => trickCards.Sum(card => card.Points));
            roundScores.Add(roundScore);
        }

        if (roundScores.Count(score => score == 0) == 3)
        {
            int iPlayerShotTheMoon = roundScores.FindIndex(score => score != 0);
            PublishGameEvent(new HeartsGameEvent.ShotTheMoon(players[iPlayerShotTheMoon].AccountCard));
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
            PublishGameEvent(new HeartsGameEvent.TrickScored(players[i].AccountCard, roundScores[i], gameState.Players[i].Score));
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
                PublishGameEvent(new GameEvent.Loser(players[i].AccountCard));
                continue;
            }

            logger.LogInformation("{AccountCard} is the winner with {TotalPoints}", players[i].AccountCard, playerState.Score);
            PublishGameEvent(new GameEvent.Winner(players[i].AccountCard));
        }
    }
}