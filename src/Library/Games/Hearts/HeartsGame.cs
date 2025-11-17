using System.Diagnostics;

using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Games.Hearts;

// TODO: since events are logged, is there anything else we'd wanna actually log (besides the log to add the game ID to the log scope)?
//       tldr: review all logs and see if they can/should be events

// TODO: mv log scoping to base class? would need a settings base class too
//       what about user vs system settings? support both in Game?

// TODO: need to kick off chanFanOut
//       only want to kick off if game starts tho, otherwise will leak channels
//       how will gameEventsChan get closed?
//       make a GameHarness proxy that kicks off all svcs and handles closing everything down?
//           make Game disposable and have GameHarness have startup+disposal Func<Task> lists?
//           merge GameHarness and HeartsGameFactory?

// TODO: the code is multi-braided-ish: it does something, and it pushes a notification to do it

// TODO: update the PlayCard(s) funcs to pass additional, human-readable validation info

// TODO: how handle data visibility to diff players?

// TODO: decompose Hearts class to make it easier to test

// TODO: revisit HeartsGame and HeartsGameFactory and see how they can be reused n cleaned up

/// <remarks>
/// It is intended to use <see cref="HeartsGameFactory"/> to instantiate this service.
/// </remarks>
public class HeartsGame : Game<HeartsCard, HeartsPlayerState>
{
    private readonly IGameEventPublisher _gameEventPublisher;
    private readonly HeartsGameState _gameState;
    private readonly IReadOnlyList<IPlayer<HeartsCard>> _players;
    private readonly IDealer _dealer;
    private readonly HeartsSettings _settings;
    private readonly ILogger<HeartsGame> _logger;

    /// <remarks>
    /// It is intended to use <see cref="HeartsGameFactory"/> to instantiate this service.
    /// </remarks>
    public HeartsGame(
        IGameEventPublisher gameEventPublisher,
        HeartsGameState gameState,
        IReadOnlyList<IPlayer<HeartsCard>> players,
        IDealer dealer,
        HeartsSettings settings,
        ILogger<HeartsGame> logger)
        : base(gameEventPublisher, gameState, players, logger)
    {
        _gameEventPublisher = gameEventPublisher;
        _gameState = gameState;
        _players = players;
        _dealer = dealer;
        _settings = settings;
        _logger = logger;
    }

    public const int NumPlayers = 4;

    public override string Name => "Hearts";

    protected override async Task ActuallyPlay(CancellationToken cancellationToken)
    {
        using var loggingScope = _logger.BeginScope(
            "Hearts game with ID {GameId} and settings {Settings}",
            Guid.NewGuid(), _settings);
        _logger.LogInformation("Beginning a hearts game");

        for (var i = 0; i < _players.Count; i++)
            _logger.LogInformation("Player at index {PlayerIndex} is {PlayerCard}", i, _players[i].PlayerAccountCard);

        var dealerPosition = new CircularCounter(4, startAtEnd: true);
        while (_gameState.Players.All(player => player.Score < _settings.EndOfGamePoints))
        {
            await SetupRound((PassDirection)dealerPosition.Tick(), cancellationToken);

            _gameState.IndexTrickStartPlayer = _gameState.Players.FindIndex(player =>
                player.Hand.Any(card => card.Value is TwoOfClubs));
            if (_gameState.IndexTrickStartPlayer == -1)
                throw new InvalidOperationException($"Could not find a player with the {nameof(TwoOfClubs)}");

            _gameState.IsFirstTrick = true;
            while (_gameState.Players[0].Hand.Any())
            {
                await PlayOutTrick(cancellationToken);
                _gameState.IsFirstTrick = false;
            }

            if (_gameState.Players.Any(player => player.Hand.Any()))
                throw new InvalidOperationException("Some players have cards left despite the 0th player having none");

            await ScoreTricks(cancellationToken);
        }

        await DetermineAndPublishWinnersAndLosers(cancellationToken);
        await _gameEventPublisher.Publish(new GameEvent.GameEnded(Name, CompletedNormally: true), cancellationToken);
    }

    private async Task SetupRound(PassDirection passDirection, CancellationToken cancellationToken)
    {
        _gameState.IsHeartsBroken = false;
        await _gameEventPublisher.Publish(GameEvent.SettingUpNewRound.Singleton, cancellationToken);

        _logger.LogInformation("Shuffling, cutting, and dealing the deck to {NumPlayers}",
            NumPlayers);
        // TODO: could preserve and reshuffle cards instead of reinstantiating every round
        List<Cards<HeartsCard>> hands = await _dealer.ShuffleCutDeal(
            deck: HeartsCard.MakeDeck(Decks.Standard52()),
            numHands: NumPlayers,
            cancellationToken);

        for (int i = 0; i < NumPlayers; i++)
        {
            _gameState.Players[i].Hand = hands[i];
            await _gameEventPublisher.Publish(new GameEvent.HandGiven(_players[i].PlayerAccountCard, hands[i].Count), cancellationToken);
        }

        if (passDirection is PassDirection.Hold)
        {
            await _gameEventPublisher.Publish(HeartsGameEvent.HoldEmRound.Singleton, cancellationToken);
            return;
        }

        var passingEventEnvelope = await _gameEventPublisher.Publish(new HeartsGameEvent.GetReadyToPass(passDirection), cancellationToken);
        _logger.LogInformation("Asking each player to select three cards to pass {PassDirection}",
            passDirection);
        List<Task<Cards<HeartsCard>>> takeCardsFromPlayerTasks = new(capacity: NumPlayers);
        for (int i = 0; i < NumPlayers; i++)
        {
            Task<Cards<HeartsCard>> task = PromptForValidCardsAndPlay(
                iPlayer: i,
                validateChosenCards: (_, iCardsToPlay) => iCardsToPlay.Count == 3,
                cancellationToken);
            takeCardsFromPlayerTasks.Add(task);
        }

        await Task.WhenAll(takeCardsFromPlayerTasks).WaitAsync(cancellationToken);

        _logger.LogInformation("Passing cards");
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
            _gameState.Players[iTargetPlayer].Hand.AddRange(cardsToPass);
        }

        await _gameEventPublisher.Publish(new HeartsGameEvent.CardsPassed(passDirection), cancellationToken);
        await _gameEventPublisher.Publish(GameEvent.BeginningNewRound.Singleton, cancellationToken);
    }

    private async Task PlayOutTrick(CancellationToken cancellationToken)
    {
        var iTrickPlayer = new CircularCounter(seed: _gameState.IndexTrickStartPlayer, maxExclusive: NumPlayers);
        while (true) // TODO: at risk of infinite loop
        {
            _logger.LogInformation("Getting trick's opening card from {AccountCard}", _players[iTrickPlayer.N].PlayerAccountCard);
            if (!_gameState.IsHeartsBroken && _gameState.Players[_gameState.IndexTrickStartPlayer].Hand.All(card => card.Value.Suit is Suit.Hearts))
            {
                _logger.LogInformation(
                    "Hearts has not been broken and {AccountCard} only has hearts, skipping to the next player",
                    _players[iTrickPlayer.N].PlayerAccountCard);
                iTrickPlayer.CycleClockwise();
            }
            else
                break;
        }

        var openingCard = await PromptForValidCardAndPlay(
            iPlayer: _gameState.IndexTrickStartPlayer,
            validateChosenCard: (hand, iCardToPlay) => _gameState.IsFirstTrick
                ? hand[iCardToPlay].Value is TwoOfClubs
                : _gameState.IsHeartsBroken || hand[iCardToPlay].Value.Suit is not Suit.Hearts,
            cancellationToken);
        var trick = new Cards<HeartsCard>(capacity: NumPlayers) { openingCard };
        var suitToFollow = openingCard.Value.Suit;

        while (iTrickPlayer.CycleClockwise() != _gameState.IndexTrickStartPlayer)
        {
            HeartsCard chosenCard = await PromptForValidCardAndPlay(
                iPlayer: iTrickPlayer.N,
                validateChosenCard: (hand, iCardToPlay) =>
                {
                    if (!CheckPlayedCard.IsSuitFollowedIfPossible(suitToFollow, hand, iCardToPlay))
                        return false;
                    if (_gameState.IsFirstTrick && hand[iCardToPlay].Points != 0)
                        return false;
                    return true;
                },
                cancellationToken);
            trick.Add(chosenCard);

            if (!_gameState.IsHeartsBroken && chosenCard.Value.Suit is Suit.Hearts)
            {
                await _gameEventPublisher.Publish(new HeartsGameEvent.HeartsHaveBeenBroken(_players[iTrickPlayer.N].PlayerAccountCard, chosenCard), cancellationToken);
                _gameState.IsHeartsBroken = true;
            }
        }

        if (trick.Count != NumPlayers)
            throw new InvalidOperationException($"After playing a trick, the trick has {trick.Count} cards but expected {NumPlayers} cards");

        IEnumerable<HeartsCard> onSuitCards = trick.Where(card => card.Value.Suit == suitToFollow);
        Rank highestOnSuitRank = GetHighest.Of(HeartsRankPriorities.Value, onSuitCards.Select(card => card.Value.Rank).ToList());
        int iTrickTakerOffsetFromStartPlayer = trick.FindIndex(card => card.Value.Suit == suitToFollow && card.Value.Rank == highestOnSuitRank);
        if (iTrickTakerOffsetFromStartPlayer == -1)
            throw new InvalidOperationException($"Could not find a card in the trick with suit {suitToFollow} and rank {highestOnSuitRank}");
        int iNextTrickStartPlayer = new CircularCounter(seed: _gameState.IndexTrickStartPlayer, maxExclusive: NumPlayers)
            .Tick(delta: iTrickTakerOffsetFromStartPlayer);
        await _gameEventPublisher.Publish(
            new GameEvent.PlayerTookTrickWithCard<HeartsCard>(_players[iNextTrickStartPlayer].PlayerAccountCard, trick[iTrickTakerOffsetFromStartPlayer]),
            cancellationToken);
        _gameState.Players[iNextTrickStartPlayer].TricksTaken.Add(trick);
        _gameState.IndexTrickStartPlayer = iNextTrickStartPlayer;
    }

    private async Task ScoreTricks(CancellationToken cancellationToken)
    {
        await _gameEventPublisher.Publish(GameEvent.ScoringRound.Singleton, cancellationToken);
        List<int> roundScores = new(capacity: NumPlayers);
        foreach (HeartsPlayerState playerState in _gameState.Players)
        {
            int roundScore = playerState.TricksTaken.Sum(trickCards => trickCards.Sum(card => card.Points));
            roundScores.Add(roundScore);
        }

        if (roundScores.Count(score => score == 0) == 3)
        {
            int iPlayerShotTheMoon = roundScores.FindIndex(score => score != 0);
            await _gameEventPublisher.Publish(new HeartsGameEvent.ShotTheMoon(_players[iPlayerShotTheMoon].PlayerAccountCard), cancellationToken);
            const int totalPointsInDeck = 26;
            for (int i = 0; i < roundScores.Count; i++)
            {
                if (i != iPlayerShotTheMoon)
                    roundScores[i] = totalPointsInDeck;
            }
        }

        for (int i = 0; i < roundScores.Count; i++)
        {
            _gameState.Players[i].Score += roundScores[i];
            await _gameEventPublisher.Publish(new HeartsGameEvent.TrickScored(_players[i].PlayerAccountCard, roundScores[i], _gameState.Players[i].Score), cancellationToken);
        }
    }

    private async Task DetermineAndPublishWinnersAndLosers(CancellationToken cancellationToken)
    {
        for (int i = 0; i < _gameState.Players.Count; i++)
        {
            HeartsPlayerState playerState = _gameState.Players[i];
            if (playerState.Score < _settings.EndOfGamePoints)
                continue;
            _logger.LogInformation("{AccountCard} is at or over {EndOfGamePoints} points with {TotalPoints}",
                _players[i].PlayerAccountCard, _settings.EndOfGamePoints, playerState.Score);
        }

        int minScore = _gameState.Players.Min(player => player.Score);
        for (int i = 0; i < _gameState.Players.Count; i++)
        {
            HeartsPlayerState playerState = _gameState.Players[i];
            if (playerState.Score != minScore)
            {
                await _gameEventPublisher.Publish(new GameEvent.Loser(_players[i].PlayerAccountCard), cancellationToken);
                continue;
            }

            _logger.LogInformation("{AccountCard} is the winner with {TotalPoints}", _players[i].PlayerAccountCard, playerState.Score);
            await _gameEventPublisher.Publish(new GameEvent.Winner(_players[i].PlayerAccountCard), cancellationToken);
        }
    }
}