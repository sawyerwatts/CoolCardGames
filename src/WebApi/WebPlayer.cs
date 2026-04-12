using CoolCardGames.Library.Core.CardTypes;
using CoolCardGames.Library.Core.CardUtils;
using CoolCardGames.Library.Core.MiscUtils;
using CoolCardGames.Library.Core.Players;
using CoolCardGames.WebApi.Endpoints.GameSession;

namespace CoolCardGames.WebApi;

/// <summary>
/// Games will asynchronously ask this player class for cards, and in parallel, users will make web
/// requests to get events and submit cards. Beyond the usual responsibilities of <see cref="IPlayer"/>,
/// this class handles the synchronization of these two operations.
/// </summary>
/// <param name="playerAccountCard"></param>
/// <param name="logger"></param>
public class WebPlayer(
    PlayerAccountCard playerAccountCard,
    ILogger<IPlayer> logger)
    : Player(logger)
{
    public override PlayerAccountCard AccountCard => playerAccountCard;

    /// <summary>
    /// Because this class has two parallel callers (Game and the API controller), this lock is used
    /// to ensure only one caller is making requests at once.
    /// </summary>
    private readonly AsyncLock _asyncLock = new(logger);

    private IEnumerable<string>? IfNotNullSelectCardFollowingTheseRules { get; set; }
    private IEnumerable<string>? IfNotNullSelectCardComboFollowingTheseRules { get; set; }
    // TODO: need a pointer to internal game state so can get cards on demand
    //      have its own attach to game? update Player.JoinGame to take a pointer to the game state?
    //          update JoinGame and pass a func (method on GameState) to return state visible to given player?

    public async Task<GameSessionGetCurrentStateResponse> GetCurrentState(CancellationToken cancellationToken)
    {
        return await _asyncLock.LockThenExecute(nameof(GetCurrentState), async () =>
        {
            if (CurrGameEvents is null)
                throw new InvalidOperationException("Cannot get new game events when the player isn't attached to a game");

            var result = new GameSessionGetCurrentStateResponse();

            if (IfNotNullSelectCardFollowingTheseRules is not null)
                result.IfNotNullSelectCardFollowingTheseRules = IfNotNullSelectCardFollowingTheseRules;

            if (IfNotNullSelectCardComboFollowingTheseRules is not null)
                result.IfNotNullSelectCardComboFollowingTheseRules = IfNotNullSelectCardComboFollowingTheseRules;

            // BUG: if the network drops the response, then the user won't have any way of replaying
            //      the lost events. I couldn't think of a relatively painless way to implement event
            //      replaying logic that wasn't also susceptible to replaying cheating.
            while (CurrGameEvents.TryPeek(out _))
            {
                var envelope = await CurrGameEvents.ReadAsync(cancellationToken);
                result.NewGameEvents.Add(envelope);
            }

            return result;
        });
    }

    public Task AnswerPromptForIndexOfCardToPlay(uint prePromptEventId, Cards cards, List<CardSelectionRule> cardSelectionRules, CancellationToken cancellationToken)
    {
        // TODO: this
        IfNotNullSelectCardFollowingTheseRules = cardSelectionRules.Select(rule => rule.Description);
        throw new NotImplementedException();
    }

    public Task AnswerPromptForIndexesOfCardsToPlay(uint prePromptEventId, Cards cards, List<CardComboSelectionRule> cardComboSelectionRules, CancellationToken cancellationToken)
    {
        // TODO: this
        IfNotNullSelectCardComboFollowingTheseRules = cardComboSelectionRules.Select(rule => rule.Description);
        throw new NotImplementedException();
    }

    protected override Task<int> PromptForIndexOfCardToPlay(uint prePromptEventId, Cards cards, List<CardSelectionRule> cardSelectionRules, CancellationToken cancellationToken)
    {
        // TODO: this
        throw new NotImplementedException();
    }

    protected override Task<List<int>> PromptForIndexesOfCardsToPlay(uint prePromptEventId, Cards cards, List<CardComboSelectionRule> cardComboSelectionRules, CancellationToken cancellationToken)
    {
        // TODO: this
        throw new NotImplementedException();
    }

    protected override Task CardSelectedWasNotValid(Cards cards, int iCardSelected, List<string> rulesFailed, CancellationToken cancellationToken)
    {
        // TODO: this
        throw new NotImplementedException();
    }

    protected override Task CardsSelectedWereNotValid(Cards cards, List<int> iCardsSelected, List<string> rulesFailed, CancellationToken cancellationToken)
    {
        // TODO: this
        throw new NotImplementedException();
    }
}