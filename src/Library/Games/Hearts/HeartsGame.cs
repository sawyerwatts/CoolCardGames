using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Games.Hearts;

/// <remarks>
/// It is intended to use <see cref="HeartsGameFactory"/> to instantiate this service.
/// </remarks>
public sealed class HeartsGame : Game<HeartsPlayerState>
{
    private readonly IGameEventPublisher _gameEventPublisher;
    private readonly HeartsGameState _gameState;
    private readonly IHeartsSetupRound _setupRound;
    private readonly IReadOnlyList<IPlayer> _players;
    private readonly HeartsSettings _settings;

    /// <remarks>
    /// It is intended to use <see cref="HeartsGameFactory"/> to instantiate this service.
    /// </remarks>
    public HeartsGame(
        IGameEventPublisher gameEventPublisher,
        HeartsGameState gameState,
        IHeartsSetupRound setupRound,
        IReadOnlyList<IPlayer> players,
        HeartsSettings settings,
        ILogger<HeartsGame> logger)
        : base(gameEventPublisher, gameState, players, logger)
    {
        _gameEventPublisher = gameEventPublisher;
        _gameState = gameState;
        _setupRound = setupRound;
        _players = players;
        _settings = settings;
    }

    public const int NumPlayers = 4;

    public const string NameConst = "Hearts";
    public override string Name => NameConst;

    protected override object? SettingsToBeLogged => new { UserGameSettings = _settings };

    public override void Dispose() { }

    protected override async Task ActuallyPlay(CancellationToken cancellationToken)
    {
        var dealerPosition = new CircularCounter(4, startAtEnd: true);
        while (_gameState.Players.All(player => player.Score < _settings.EndOfGamePoints))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _setupRound.Go(_gameState, (PassDirection)dealerPosition.Tick(), cancellationToken);

            _gameState.IndexTrickStartPlayer = _gameState.Players.FindIndex(player =>
                player.Hand.Any(card => card.Value is TwoOfClubs));
            if (_gameState.IndexTrickStartPlayer == -1)
            {
                throw new InvalidOperationException($"Could not find a player with the {nameof(TwoOfClubs)}");
            }

            while (_gameState.Players[0].Hand.Count != 0)
            {
                await PlayOutTrick(cancellationToken);
                _gameState.IsFirstTrick = false;
            }

            if (_gameState.Players.Any(player => player.Hand.Count != 0))
            {
                throw new InvalidOperationException("Some players have cards left despite the 0th player having none");
            }

            await ScoreTricks(cancellationToken);
        }

        await DetermineAndPublishWinnersAndLosers(cancellationToken);
    }

    private async Task PlayOutTrick(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var iTrickPlayer = new CircularCounter(seed: _gameState.IndexTrickStartPlayer, maxExclusive: NumPlayers);
        while (true) // TODO: at risk of infinite loop
        {
            var player = _players[iTrickPlayer.Value];
            var playerState = _gameState.Players[iTrickPlayer.Value];
            await _gameEventPublisher.Publish(new HeartsGameEvent.GettingOpeningCardFrom(player.AccountCard), cancellationToken);
            if (!_gameState.IsHeartsBroken && playerState.Hand.All(card => card.Value.Suit is Suit.Hearts))
            {
                await _gameEventPublisher.Publish(new HeartsGameEvent.CannotOpenPassingAction(player.AccountCard), cancellationToken);
                iTrickPlayer.CycleClockwise();
            }
            else
                break;
        }

        var openingRule = _gameState.IsFirstTrick
            ? HeartsGameCardSelectionRules.FirstTrickOpeningCardMustBeTwoOfClubs
            : HeartsGameCardSelectionRules.HeartsCanOnlyBeLeadOnceBroken(_gameState.IsHeartsBroken);
        var openingCard = await _players[_gameState.IndexTrickStartPlayer].PromptForValidCardAndPlay(
            cards: _gameState.Players[_gameState.IndexTrickStartPlayer].Hand,
            cardSelectionRule: openingRule,
            cancellationToken);
        var trick = new Cards(capacity: NumPlayers) { openingCard };
        var suitToFollow = openingCard.Value.Suit;

        List<CardSelectionRule> followingRules =
        [
            CommonCardSelectionRules.IsSuitFollowedIfPossible(suitToFollow),
        ];
        if (_gameState.IsFirstTrick)
        {
            followingRules.Add(HeartsGameCardSelectionRules.FirstTrickCannotHavePoints);
        }

        while (iTrickPlayer.CycleClockwise() != _gameState.IndexTrickStartPlayer)
        {
            var chosenCard = await _players[iTrickPlayer.Value].PromptForValidCardAndPlay(
                cards: _gameState.Players[iTrickPlayer.Value].Hand,
                cardSelectionRules: followingRules,
                cancellationToken);
            trick.Add(chosenCard);

            if (!_gameState.IsHeartsBroken && chosenCard.Value.Suit is Suit.Hearts)
            {
                await _gameEventPublisher.Publish(
                    new HeartsGameEvent.HeartsHaveBeenBroken(_players[iTrickPlayer.Value].AccountCard, (HeartsCard)chosenCard),
                    cancellationToken);
                _gameState.IsHeartsBroken = true;
            }
        }

        if (trick.Count != NumPlayers)
        {
            throw new InvalidOperationException(
                $"After playing a trick, the trick has {trick.Count} cards but expected {NumPlayers} cards");
        }

        int iTrickTakerOffsetFromStartPlayer = DetermineTrickTakerIndexRelativeToStartPlayer(trick, suitToFollow);
        var iNextTrickStartPlayer = new CircularCounter(seed: _gameState.IndexTrickStartPlayer, maxExclusive: NumPlayers)
            .Tick(delta: iTrickTakerOffsetFromStartPlayer);
        await _gameEventPublisher.Publish(
            new GameEvent.PlayerTookTrickWithCard(_players[iNextTrickStartPlayer].AccountCard,
                trick[iTrickTakerOffsetFromStartPlayer]),
            cancellationToken);
        _gameState.Players[iNextTrickStartPlayer].TricksTaken.Add(trick);
        _gameState.IndexTrickStartPlayer = iNextTrickStartPlayer;

        return;

        // TODO: pull into a helper or something?
        static int DetermineTrickTakerIndexRelativeToStartPlayer(Cards trick, Suit suitToFollow)
        {
            var onSuitCards = trick.Where(card => card.Value.Suit == suitToFollow);
            var highestOnSuitRank =
                GetHighest.Of(CommonRankPriorities.AceHighDescending, onSuitCards.Select(card => card.Value.Rank).ToList());
            var iTrickTakerOffsetFromStartPlayer = trick.FindIndex(card =>
                card.Value.Suit == suitToFollow && card.Value.Rank == highestOnSuitRank);
            if (iTrickTakerOffsetFromStartPlayer == -1)
            {
                throw new InvalidOperationException($"Could not find a card in the trick with suit {suitToFollow} and rank {highestOnSuitRank}");
            }

            return iTrickTakerOffsetFromStartPlayer;
        }
    }

    private async Task ScoreTricks(CancellationToken cancellationToken)
    {
        await _gameEventPublisher.Publish(GameEvent.ScoringRound.Singleton, cancellationToken);
        List<int> roundScores = new(capacity: NumPlayers);
        foreach (var playerState in _gameState.Players)
        {
            var roundScore = playerState.TricksTaken.Sum(trickCards => trickCards.Sum(card => ((HeartsCard)card).Points));
            roundScores.Add(roundScore);
        }

        if (roundScores.Count(score => score == 0) == 3)
        {
            var iPlayerShotTheMoon = roundScores.FindIndex(score => score != 0);
            await _gameEventPublisher.Publish(
                new HeartsGameEvent.ShotTheMoon(_players[iPlayerShotTheMoon].AccountCard), cancellationToken);
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
                new HeartsGameEvent.TrickScored(_players[i].AccountCard, roundScores[i],
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
                new GameEvent.PlayerAtOrExceededMaxPoints(_players[i].AccountCard, playerState.Score,
                    _settings.EndOfGamePoints), cancellationToken);
        }

        var minScore = _gameState.Players.Min(player => player.Score);
        for (var i = 0; i < _gameState.Players.Count; i++)
        {
            var playerState = _gameState.Players[i];
            if (playerState.Score != minScore)
            {
                await _gameEventPublisher.Publish(new GameEvent.Loser(_players[i].AccountCard),
                    cancellationToken);
                continue;
            }

            await _gameEventPublisher.Publish(new GameEvent.Winner(_players[i].AccountCard), cancellationToken);
        }
    }
}