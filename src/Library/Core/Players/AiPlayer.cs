using System.Security.Cryptography;
using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.Players;

public class AiPlayer(PlayerAccountCard playerAccountCard, ILogger<AiPlayer> logger) : Player(logger)
{
    public override PlayerAccountCard AccountCard => playerAccountCard;

    public ChannelReader<GameEventEnvelope>? CurrentGamesEvents { get; set; }

    protected override Task<int> PromptForIndexOfCardToPlay(
        uint prePromptEventId,
        Cards cards,
        List<CardSelectionRule> cardSelectionRules,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count));
    }

    protected override Task<List<int>> PromptForIndexesOfCardsToPlay(
        uint prePromptEventId,
        Cards cards,
        List<CardComboSelectionRule> cardComboSelectionRules,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<List<int>>(
        [
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count),
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count),
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count),
        ]);
    }

    protected override Task CardSelectedWasNotValid(Cards cards, int iCardSelected, List<string> rulesFailed, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override Task CardsSelectedWereNotValid(Cards cards, List<int> iCardsSelected, List<string> rulesFailed, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}