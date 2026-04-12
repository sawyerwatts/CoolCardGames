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

            if (_state.Hand is not null)
                result.Hand = _state.Hand;

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
        var resetStateThreadUnsafe = await _asyncLock.LockThenExecute(nameof(PromptForIndexOfCardToPlay), () =>
        {
            _state.IfNotNullSelectCardFollowingTheseRules = cardSelectionRules.Select(rule => rule.Description);
            _state.Hand = cards;
            _state.IndexOfCardToPlay = null;
            var cleanup = new Disposable(() =>
            {
                _state.IfNotNullSelectCardFollowingTheseRules = null;
                _state.Hand = null;
                _state.IndexOfCardToPlay = null;
            });
            return Task.FromResult(cleanup);
        });

        try
        {
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
                    resetStateThreadUnsafe.Dispose();
                    return Task.FromResult(indexOfCardToPlay);
                });

                if (indexOfCardToPlay is not null)
                    return indexOfCardToPlay.Value;

                _logger.LogInformation("Player has not responded, sleeping for {PollMs} ms", _settings.Value.PollMs);
                await Task.Delay(_settings.Value.PollMs, cancellationToken);
            }
        }
        catch (Exception exc)
        {
            _logger.LogError(exc, "An exception occurred in {PromptForIndexOfCardToPlay}, performing cleanup", nameof(PromptForIndexOfCardToPlay));
            _ = await _asyncLock.LockThenExecute(nameof(PromptForIndexOfCardToPlay), () =>
            {
                resetStateThreadUnsafe.Dispose();
                return Task.FromResult(true);
            });
            _logger.LogInformation(exc, "Cleanup completed, rethrowing");
            throw;
        }
    }

    protected override async Task<List<int>> PromptForIndexesOfCardsToPlay(uint prePromptEventId, Cards cards, List<CardComboSelectionRule> cardComboSelectionRules, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Preparing to track that the user needs to choose the index(es) of the card(s) to play");
        var resetStateThreadUnsafe = await _asyncLock.LockThenExecute(nameof(PromptForIndexesOfCardsToPlay), () =>
        {
            _state.IfNotNullSelectCardComboFollowingTheseRules = cardComboSelectionRules.Select(rule => rule.Description);
            _state.Hand = cards;
            _state.IndexesOfCardsToPlay = null;
            var cleanup = new Disposable(() =>
            {
                _state.IfNotNullSelectCardComboFollowingTheseRules = null;
                _state.Hand = null;
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
                // response into AnswerPromptForIndexOfCardToPlay. Poll until that has been done.
                _logger.LogInformation("Checking if the player has selected the index(es) of the card(s) to play");
                List<int>? indexesOfCardToPlay = await _asyncLock.LockThenExecute<List<int>?>(nameof(PromptForIndexesOfCardsToPlay), () =>
                {
                    List<int>? indexesOfCardsToPlay = _state.IndexesOfCardsToPlay;
                    if (indexesOfCardsToPlay is null)
                    {
                        indexesOfCardsToPlay = null;
                        return Task.FromResult(indexesOfCardsToPlay);
                    }

                    _logger.LogInformation("Preparing to track that the user has selected the index(es) of the card(s) to play");
                    resetStateThreadUnsafe.Dispose();
                    return Task.FromResult<List<int>?>(indexesOfCardsToPlay);
                });

                if (indexesOfCardToPlay is not null)
                    return indexesOfCardToPlay;

                _logger.LogInformation("Player has not responded, sleeping for {PollMs} ms", _settings.Value.PollMs);
                await Task.Delay(_settings.Value.PollMs, cancellationToken);
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

    protected override Task CardSelectedWasNotValid(Cards cards, int iCardSelected, List<string> rulesFailed, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task CardsSelectedWereNotValid(Cards cards, List<int> iCardsSelected, List<string> rulesFailed, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task AnswerPromptForIndexOfCardToPlay(int indexOfCardToPlay, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AnswerPromptForIndexesOfCardsToPlay(List<int> indexesOfCardsToPlay, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<GameSessionPlayCardResponse> CheckIfCardSelectedWasNotValid(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<GameSessionPlayCardsResponse> CheckIfCardsSelectedWereNotValid(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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
        public Cards? Hand { get; set; }
        public int? IndexOfCardToPlay { get; set; }
        public List<int>? IndexesOfCardsToPlay { get; set; }
    }

    public sealed class Settings
    {
        [Range(50, 10_000)] public int PollMs { get; set; } = 250;
    }
}