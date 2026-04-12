using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;

using CoolCardGames.Library.Core.CardTypes;
using CoolCardGames.Library.Core.CardUtils;
using CoolCardGames.Library.Core.GameEventTypes;
using CoolCardGames.Library.Core.GameEventUtils;
using CoolCardGames.Library.Core.MiscUtils;
using CoolCardGames.Library.Core.Players;
using CoolCardGames.WebApi.Endpoints.GameSession;

using Microsoft.Extensions.Options;

namespace CoolCardGames.WebApi;

// TODO: if they PlayCard when need to PlayCards, don't time out

// TODO: need better docs about the specific order these methods need to be used in

/// <summary>
/// Games will asynchronously ask this player class for cards, and in parallel, users will make web
/// requests to get events and submit cards. Beyond the usual responsibilities of <see cref="IPlayer"/>,
/// this class handles the synchronization of these two operations.
/// </summary>
public class WebPlayer : Player
{
    public WebPlayer(
        PlayerAccountCard playerAccountCard,
        IOptions<Settings> settings,
        ILogger<IPlayer> logger)
        : base(logger)
    {
        _playerAccountCard = playerAccountCard;
        _settings = settings;
        _asyncLock = new AsyncLock(logger);
        _logger = logger;
    }

    public override PlayerAccountCard AccountCard => _playerAccountCard;
    private readonly PlayerAccountCard _playerAccountCard;

    /// <summary>
    /// Because this class has two parallel callers (Game and the API controller), this lock is used
    /// to ensure only one caller is making requests at once.
    /// </summary>
    private readonly AsyncLock _asyncLock;

    private readonly ILogger<IPlayer> _logger;
    private readonly IOptions<Settings> _settings;

    private SharedState _state = new();

    // TODO: need a pointer to internal game state so can get cards on demand
    //      have its own attach to game? update Player.JoinGame to take a pointer to the game state?
    //          update JoinGame and pass a func (method on GameState) to return state visible to given player?

    public override Disposable JoinGame(ChannelReader<GameEventEnvelope> currGamesEvents, IGameEventPublisher currGameEventPublisher)
    {
        var cleanup = base.JoinGame(currGamesEvents, currGameEventPublisher);
        _state = new SharedState();
        return cleanup;
    }

    public async Task<GameSessionGetCurrentStateResponse> GetCurrentState(CancellationToken cancellationToken)
    {
        return await _asyncLock.LockThenExecute(nameof(GetCurrentState), async () =>
        {
            if (CurrGameEvents is null)
                throw new InvalidOperationException("Cannot get new game events when the player isn't attached to a game");

            var result = new GameSessionGetCurrentStateResponse();

            if (_state.IfNotNullSelectCardFollowingTheseRules is not null)
                result.IfNotNullSelectCardFollowingTheseRules = _state.IfNotNullSelectCardFollowingTheseRules;

            if (_state.IfNotNullSelectCardComboFollowingTheseRules is not null)
                result.IfNotNullSelectCardComboFollowingTheseRules = _state.IfNotNullSelectCardComboFollowingTheseRules;

            if (_state.Cards is not null)
                result.Cards = _state.Cards;

            // If the network drops the response, then the user won't have any way of replaying the
            // lost events. I couldn't think of a (relatively painless) way to implement event
            // replaying logic that wasn't also susceptible to replaying cheating.
            while (CurrGameEvents.TryPeek(out _))
            {
                var envelope = await CurrGameEvents.ReadAsync(cancellationToken);
                result.NewGameEventEnvelopes.Add(envelope);
            }

            return result;
        });
    }

    protected override async Task<int> PromptForIndexOfCardToPlay(uint prePromptEventId, Cards cards, List<CardSelectionRule> cardSelectionRules, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Preparing to track that the user needs to choose the index of the card to play");
        await _asyncLock.LockThenExecute(nameof(PromptForIndexOfCardToPlay), () =>
        {
            _state.IfNotNullSelectCardFollowingTheseRules = cardSelectionRules.Select(rule => rule.Description);
            _state.Cards = cards;
            _state.IndexOfCardToPlay = null;
            return Task.FromResult(true);
        });

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // In parallel, the user will be using the API to GET the current status, and then POST the
            // response into AnswerPromptForIndexOfCardToPlay. Poll until that has been done.
            _logger.LogInformation("Checking if the player has selected the index of the card to play");
            int? indexOfCardToPlay = await _asyncLock.LockThenExecute(nameof(PromptForIndexOfCardToPlay), () =>
            {
                int? indexOfCardToPlay = _state.IndexOfCardToPlay;
                if (indexOfCardToPlay is null)
                {
                    int? noResp = null;
                    return Task.FromResult(noResp);
                }

                _logger.LogInformation("Preparing to track that the user has selected the index of the card to play");
                return Task.FromResult(indexOfCardToPlay);
            });

            if (indexOfCardToPlay is not null)
                return indexOfCardToPlay.Value;

            _logger.LogInformation("Player has not responded, sleeping for {PollMs} ms", _settings.Value.UserResponsePollMs);
            await Task.Delay(_settings.Value.UserResponsePollMs, cancellationToken);
        }
    }

    protected override async Task<List<int>> PromptForIndexesOfCardsToPlay(uint prePromptEventId, Cards cards, List<CardComboSelectionRule> cardComboSelectionRules, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Preparing to track that the user needs to choose the index(es) of the card(s) to play");
        var resetStateThreadUnsafe = await _asyncLock.LockThenExecute(nameof(PromptForIndexesOfCardsToPlay), () =>
        {
            _state.IfNotNullSelectCardComboFollowingTheseRules = cardComboSelectionRules.Select(rule => rule.Description);
            _state.Cards = cards;
            _state.IndexesOfCardsToPlay = null;
            var cleanup = new Disposable(() =>
            {
                _state.IfNotNullSelectCardComboFollowingTheseRules = null;
                _state.Cards = null;
                _state.IndexesOfCardsToPlay = null;
            });
            return Task.FromResult(cleanup);
        });

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // In parallel, the user will be using the API to GET the current status, and then POST the
                // response into AnswerPromptForIndexesOfCardsToPlay. Poll until that has been done.
                _logger.LogInformation("Checking if the player has selected the index(es) of the card(s) to play");
                List<int>? indexesOfCardToPlay = await _asyncLock.LockThenExecute(nameof(PromptForIndexesOfCardsToPlay), () =>
                {
                    List<int>? indexesOfCardsToPlay = _state.IndexesOfCardsToPlay;
                    if (indexesOfCardsToPlay is null)
                        return Task.FromResult(indexesOfCardsToPlay);

                    _logger.LogInformation("Preparing to track that the user has selected the index(es) of the card(s) to play");
                    resetStateThreadUnsafe.Dispose();
                    return Task.FromResult<List<int>?>(indexesOfCardsToPlay);
                });

                if (indexesOfCardToPlay is not null)
                    return indexesOfCardToPlay;

                _logger.LogInformation("Player has not responded, sleeping for {PollMs} ms", _settings.Value.UserResponsePollMs);
                await Task.Delay(_settings.Value.UserResponsePollMs, cancellationToken);
            }
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "An exception occurred in {PromptForIndexesOfCardsToPlay}, performing cleanup", nameof(PromptForIndexesOfCardsToPlay));
            _ = await _asyncLock.LockThenExecute(nameof(PromptForIndexesOfCardsToPlay), () =>
            {
                resetStateThreadUnsafe.Dispose();
                return Task.FromResult(true);
            });
            _logger.LogInformation(exc, "Cleanup completed, rethrowing");
            throw;
        }
    }

    public async Task AnswerPromptForIndexOfCardToPlay(int indexOfCardToPlay, CancellationToken cancellationToken)
    {
        await _asyncLock.LockThenExecute(nameof(AnswerPromptForIndexOfCardToPlay), () =>
        {
            _state.IndexOfCardToPlay = indexOfCardToPlay;
            _state.GameReviewedCardOrCardsToPlay = false;
            return Task.FromResult(true);
        });
    }

    public async Task AnswerPromptForIndexesOfCardsToPlay(List<int> indexesOfCardsToPlay, CancellationToken cancellationToken)
    {
        await _asyncLock.LockThenExecute(nameof(AnswerPromptForIndexesOfCardsToPlay), () =>
        {
            _state.IndexesOfCardsToPlay = indexesOfCardsToPlay;
            _state.GameReviewedCardOrCardsToPlay = false;
            return Task.FromResult(true);
        });
    }

    public override async Task<Card> PromptForValidCardAndPlay(Cards cards, List<CardSelectionRule> cardSelectionRules, CancellationToken cancellationToken, bool reveal = true)
    {
        var selectedCard = await base.PromptForValidCardAndPlay(cards, cardSelectionRules, cancellationToken, reveal);
        await _asyncLock.LockThenExecute(nameof(PromptForValidCardAndPlay), () =>
        {
            _state.GameReviewedCardOrCardsToPlay = true;
            return Task.FromResult(true);
        });
        return selectedCard;
    }

    public override async Task<Cards> PromptForValidCardsAndPlay(Cards cards, List<CardComboSelectionRule> cardComboSelectionRules, CancellationToken cancellationToken, bool reveal = true)
    {
        var selectedCards = await base.PromptForValidCardsAndPlay(cards, cardComboSelectionRules, cancellationToken, reveal);
        await _asyncLock.LockThenExecute(nameof(PromptForValidCardAndPlay), () =>
        {
            _state.GameReviewedCardOrCardsToPlay = true;
            return Task.FromResult(true);
        });
        return selectedCards;
    }

    protected override async Task CardSelectedWasNotValid(Cards cards, int iCardSelected, List<string> rulesFailed, CancellationToken cancellationToken)
    {
        await _asyncLock.LockThenExecute(nameof(CardSelectedWasNotValid), () =>
        {
            _state.GameReviewedCardOrCardsToPlay = true;
            _state.Cards = cards;
            _state.IndexOfCardToPlay = iCardSelected;
            _state.RulesFailed = rulesFailed;
            return Task.FromResult(true);
        });
    }

    protected override async Task CardsSelectedWereNotValid(Cards cards, List<int> iCardsSelected, List<string> rulesFailed, CancellationToken cancellationToken)
    {
        await _asyncLock.LockThenExecute(nameof(CardsSelectedWereNotValid), () =>
        {
            _state.GameReviewedCardOrCardsToPlay = true;
            _state.Cards = cards;
            _state.IndexesOfCardsToPlay = iCardsSelected;
            _state.RulesFailed = rulesFailed;
            return Task.FromResult(true);
        });
    }

    public async Task<GameSessionPlayCardResponse> CheckIfCardSelectedWasNotValid(CancellationToken cancellationToken)
    {
        GameSessionPlayCardResponse? response = null;
        for (int i = 0; response is null && i < _settings.Value.GameReviewCardsMaxPolls; i++)
        {
            await Task.Delay(_settings.Value.GameReviewCardsPollMs, cancellationToken);

            response = await _asyncLock.LockThenExecute(nameof(CheckIfCardSelectedWasNotValid), async () =>
            {
                var reviewed = _state.GameReviewedCardOrCardsToPlay;
                if (!reviewed)
                    return null;

                _state.GameReviewedCardOrCardsToPlay = false;

                var resp = new GameSessionPlayCardResponse();
                resp.AcceptedCardPlayed = _state.RulesFailed is null;
                if (resp.AcceptedCardPlayed)
                    return resp;

                resp.RulesFailed = _state.RulesFailed;
                _state.RulesFailed = null;

                resp.IndexOfCardAttempted = _state.IndexOfCardToPlay;
                _state.IndexOfCardToPlay = null;

                resp.AllCards = _state.Cards;
                _state.Cards = null;

                return resp;
            });
        }

        if (response is null)
            throw new InvalidOperationException("The game never reviewed the cards before timing out");
        return response;
    }

    public async Task<GameSessionPlayCardsResponse> CheckIfCardsSelectedWereNotValid(CancellationToken cancellationToken)
    {
        GameSessionPlayCardsResponse? response = null;
        for (int i = 0; response is null && i < _settings.Value.GameReviewCardsMaxPolls; i++)
        {
            await Task.Delay(_settings.Value.GameReviewCardsPollMs, cancellationToken);

            response = await _asyncLock.LockThenExecute(nameof(CheckIfCardsSelectedWereNotValid), async () =>
            {
                var reviewed = _state.GameReviewedCardOrCardsToPlay;
                if (!reviewed)
                    return null;

                _state.GameReviewedCardOrCardsToPlay = false;

                var resp = new GameSessionPlayCardsResponse();
                resp.AcceptedCardsPlayed = _state.RulesFailed is null;
                if (resp.AcceptedCardsPlayed)
                    return resp;

                resp.RulesFailed = _state.RulesFailed;
                _state.RulesFailed = null;

                resp.IndexesOfCardsAttempted = _state.IndexesOfCardsToPlay;
                _state.IndexesOfCardsToPlay = null;

                resp.AllCards = _state.Cards;
                _state.Cards = null;

                return resp;
            });
        }

        if (response is null)
            throw new InvalidOperationException("The game never reviewed the cards before timing out");
        return response;
    }

    /// <summary>
    /// Properties in this type are going to be used in multiple threads, so they should only be
    /// accessed within a <see cref="AsyncLock.LockThenExecute{T}"/> block.
    /// <br />
    /// Don't forget to reset the values after their method has completed!
    /// </summary>
    private sealed class SharedState
    {
        public IEnumerable<string>? IfNotNullSelectCardFollowingTheseRules { get; set; }
        public IEnumerable<string>? IfNotNullSelectCardComboFollowingTheseRules { get; set; }
        public Cards? Cards { get; set; }
        public int? IndexOfCardToPlay { get; set; }
        public List<int>? IndexesOfCardsToPlay { get; set; }
        public List<string>? RulesFailed { get; set; }
        public bool GameReviewedCardOrCardsToPlay { get; set; }
    }

    public sealed class Settings
    {
        [Range(50, 10_000)] public int UserResponsePollMs { get; set; } = 250;
        [Range(1, 100)] public int GameReviewCardsPollMs { get; set; } = 50;
        [Range(1, 100)] public int GameReviewCardsMaxPolls { get; set; } = 100;
    }
}