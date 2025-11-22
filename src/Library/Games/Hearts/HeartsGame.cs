using System.Diagnostics;
using CoolCardGames.Library.Core.Players;
using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Games.Hearts;

// TODO: helpers to prompt for card(s) from many/all players?

// TODO: update the PlayCard(s) funcs to pass additional, human-readable validation info

// TODO: how handle data visibility to diff players?

// TODO: decompose Hearts class to make it easier to test

// TODO: revisit HeartsGame and HeartsGameFactory and see how they can be reused n cleaned up

// TODO: cache settings somewhere?

/// <remarks>
/// It is intended to use <see cref="HeartsGameFactory"/> to instantiate this service.
/// </remarks>
public sealed class HeartsGame : Game<HeartsCard, HeartsPlayerState>
{
    private readonly IGameEventPublisher _gameEventPublisher;
    private readonly HeartsGameState _gameState;
    private readonly IReadOnlyList<IPlayer<HeartsCard>> _players;
    private readonly IDealer _dealer;
    private readonly HeartsSettings _settings;

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
    }

    public const int NumPlayers = 4;

    public override string Name => "Hearts";

    protected override object? SettingsToBeLogged => new
    {
        UserGameSettings = _settings
    };

    protected override async Task ActuallyPlay(CancellationToken cancellationToken)
    {
        var dealerPosition = new CircularCounter(4, startAtEnd: true);
        while (_gameState.Players.All(player => player.Score < _settings.EndOfGamePoints))
        {
            await SetupRound((PassDirection)dealerPosition.Tick(), cancellationToken);

            _gameState.IndexTrickStartPlayer = _gameState.Players.FindIndex(player =>
                player.Hand.Any(card => card.Value is TwoOfClubs));
            if (_gameState.IndexTrickStartPlayer == -1)
            {
                throw new InvalidOperationException($"Could not find a player with the {nameof(TwoOfClubs)}");
            }

            while (_gameState.Players[0].Hand.Any())
            {
                await PlayOutTrick(cancellationToken);
                _gameState.IsFirstTrick = false;
            }

            if (_gameState.Players.Any(player => player.Hand.Any()))
            {
                throw new InvalidOperationException("Some players have cards left despite the 0th player having none");
            }

            await ScoreTricks(cancellationToken);
        }

        await DetermineAndPublishWinnersAndLosers(cancellationToken);
        await _gameEventPublisher.Publish(new GameEvent.GameEnded(Name, CompletedNormally: true), cancellationToken);
    }

    public override void Dispose() { }

    private async Task SetupRound(PassDirection passDirection, CancellationToken cancellationToken)
    {
        await _gameEventPublisher.Publish(GameEvent.SettingUpNewRound.Singleton, cancellationToken);

        SetupRoundResetState();
        await SetupRoundInitHands(cancellationToken);

        if (passDirection is PassDirection.Hold)
        {
            await _gameEventPublisher.Publish(HeartsGameEvent.HoldEmRound.Singleton, cancellationToken);
            return;
        }

        await SetupRoundPlayersPassCards(passDirection, cancellationToken);

        await _gameEventPublisher.Publish(new HeartsGameEvent.CardsPassed(passDirection), cancellationToken);
        await _gameEventPublisher.Publish(GameEvent.BeginningNewRound.Singleton, cancellationToken);
    }

    private void SetupRoundResetState()
    {
        _gameState.IsFirstTrick = true;
        _gameState.IsHeartsBroken = false;
        foreach (var playerState in _gameState.Players)
            playerState.TricksTaken.Clear();
    }

    private async Task SetupRoundInitHands(CancellationToken cancellationToken)
    {
        // TODO: could preserve and reshuffle cards instead of reinstantiating every round
        var hands = await _dealer.ShuffleCutDeal(
            deck: HeartsCard.MakeDeck(Decks.Standard52()),
            numHands: NumPlayers,
            cancellationToken);

        for (var i = 0; i < NumPlayers; i++)
        {
            _gameState.Players[i].Hand = hands[i];
            await _gameEventPublisher.Publish(new GameEvent.HandGiven(_players[i].PlayerAccountCard, hands[i].Count),
                cancellationToken);
        }
    }

    private async Task SetupRoundPlayersPassCards(PassDirection passDirection, CancellationToken cancellationToken)
    {
        await _gameEventPublisher.Publish(new HeartsGameEvent.GetReadyToPass(passDirection), cancellationToken);
        List<Task<Cards<HeartsCard>>> takeCardsFromPlayerTasks = new(capacity: NumPlayers);
        for (var i = 0; i < NumPlayers; i++)
        {
            var task = PromptForValidCardsAndPlay(
                iPlayer: i,
                validateChosenCards: (_, iCardsToPlay) => iCardsToPlay.Count == 3,
                cancellationToken,
                reveal: false);
            takeCardsFromPlayerTasks.Add(task);
        }

        await Task.WhenAll(takeCardsFromPlayerTasks).WaitAsync(cancellationToken);

        for (var iSourcePlayer = 0; iSourcePlayer < NumPlayers; iSourcePlayer++)
        {
            CircularCounter sourcePlayerPosition = new(iSourcePlayer, NumPlayers);
            var iTargetPlayer = passDirection switch
            {
                PassDirection.Left => sourcePlayerPosition.CycleClockwise(updateInstance: false),
                PassDirection.Right => sourcePlayerPosition.CycleCounterClockwise(updateInstance: false),
                PassDirection.Across => sourcePlayerPosition.CycleClockwise(times: 2, updateInstance: false),
                _ => throw new UnreachableException(
                    $"Passing {passDirection} from {nameof(iSourcePlayer)} {iSourcePlayer}"),
            };

            var cardsToPass = takeCardsFromPlayerTasks[iSourcePlayer].Result;
            _gameState.Players[iTargetPlayer].Hand.AddRange(cardsToPass);
        }
    }

    private async Task PlayOutTrick(CancellationToken cancellationToken)
    {
        var iTrickPlayer = new CircularCounter(seed: _gameState.IndexTrickStartPlayer, maxExclusive: NumPlayers);
        while (true) // TODO: at risk of infinite loop
        {
            var player = _players[iTrickPlayer.N];
            await _gameEventPublisher.Publish(new HeartsGameEvent.GettingOpeningCardFrom(player.PlayerAccountCard), cancellationToken);
            if (!_gameState.IsHeartsBroken && _gameState.Players[iTrickPlayer.N].Hand
                    .All(card => card.Value.Suit is Suit.Hearts))
            {
                await _gameEventPublisher.Publish(new HeartsGameEvent.CannotOpenPassingAction(player.PlayerAccountCard), cancellationToken);
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
            var chosenCard = await PromptForValidCardAndPlay(
                iPlayer: iTrickPlayer.N,
                validateChosenCard: (hand, iCardToPlay) =>
                {
                    if (!CheckPlayedCard.IsSuitFollowedIfPossible(suitToFollow, hand, iCardToPlay))
                        return false;
                    return !_gameState.IsFirstTrick || hand[iCardToPlay].Points == 0;
                },
                cancellationToken);
            trick.Add(chosenCard);

            if (!_gameState.IsHeartsBroken && chosenCard.Value.Suit is Suit.Hearts)
            {
                await _gameEventPublisher.Publish(
                    new HeartsGameEvent.HeartsHaveBeenBroken(_players[iTrickPlayer.N].PlayerAccountCard, chosenCard),
                    cancellationToken);
                _gameState.IsHeartsBroken = true;
            }
        }

        if (trick.Count != NumPlayers)
        {
            throw new InvalidOperationException(
                $"After playing a trick, the trick has {trick.Count} cards but expected {NumPlayers} cards");
        }

        var onSuitCards = trick.Where(card => card.Value.Suit == suitToFollow);
        var highestOnSuitRank =
            GetHighest.Of(HeartsRankPriorities.Value, onSuitCards.Select(card => card.Value.Rank).ToList());
        var iTrickTakerOffsetFromStartPlayer = trick.FindIndex(card =>
            card.Value.Suit == suitToFollow && card.Value.Rank == highestOnSuitRank);
        if (iTrickTakerOffsetFromStartPlayer == -1)
            throw new InvalidOperationException(
                $"Could not find a card in the trick with suit {suitToFollow} and rank {highestOnSuitRank}");
        var iNextTrickStartPlayer =
            new CircularCounter(seed: _gameState.IndexTrickStartPlayer, maxExclusive: NumPlayers)
                .Tick(delta: iTrickTakerOffsetFromStartPlayer);
        await _gameEventPublisher.Publish(
            new GameEvent.PlayerTookTrickWithCard<HeartsCard>(_players[iNextTrickStartPlayer].PlayerAccountCard,
                trick[iTrickTakerOffsetFromStartPlayer]),
            cancellationToken);
        _gameState.Players[iNextTrickStartPlayer].TricksTaken.Add(trick);
        _gameState.IndexTrickStartPlayer = iNextTrickStartPlayer;
    }

    private async Task ScoreTricks(CancellationToken cancellationToken)
    {
        await _gameEventPublisher.Publish(GameEvent.ScoringRound.Singleton, cancellationToken);
        List<int> roundScores = new(capacity: NumPlayers);
        foreach (var playerState in _gameState.Players)
        {
            var roundScore = playerState.TricksTaken.Sum(trickCards => trickCards.Sum(card => card.Points));
            roundScores.Add(roundScore);
        }

        if (roundScores.Count(score => score == 0) == 3)
        {
            var iPlayerShotTheMoon = roundScores.FindIndex(score => score != 0);
            await _gameEventPublisher.Publish(
                new HeartsGameEvent.ShotTheMoon(_players[iPlayerShotTheMoon].PlayerAccountCard), cancellationToken);
            const int totalPointsInDeck = 26;
            for (var i = 0; i < roundScores.Count; i++)
            {
                if (i != iPlayerShotTheMoon)
                {
                    roundScores[i] = totalPointsInDeck;
                }
            }
        }

        for (var i = 0; i < roundScores.Count; i++)
        {
            _gameState.Players[i].Score += roundScores[i];
            await _gameEventPublisher.Publish(
                new HeartsGameEvent.TrickScored(_players[i].PlayerAccountCard, roundScores[i],
                    _gameState.Players[i].Score), cancellationToken);
        }
    }

    private async Task DetermineAndPublishWinnersAndLosers(CancellationToken cancellationToken)
    {
        for (var i = 0; i < _gameState.Players.Count; i++)
        {
            var playerState = _gameState.Players[i];
            if (playerState.Score < _settings.EndOfGamePoints)
            {
                continue;
            }

            await _gameEventPublisher.Publish(
                new GameEvent.PlayerAtOrExceededMaxPoints(_players[i].PlayerAccountCard, playerState.Score,
                    _settings.EndOfGamePoints), cancellationToken);
        }

        var minScore = _gameState.Players.Min(player => player.Score);
        for (var i = 0; i < _gameState.Players.Count; i++)
        {
            var playerState = _gameState.Players[i];
            if (playerState.Score != minScore)
            {
                await _gameEventPublisher.Publish(new GameEvent.Loser(_players[i].PlayerAccountCard),
                    cancellationToken);
                continue;
            }

            await _gameEventPublisher.Publish(new GameEvent.Winner(_players[i].PlayerAccountCard), cancellationToken);
        }
    }
}