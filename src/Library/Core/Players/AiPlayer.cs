using System.Security.Cryptography;
using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.Players;

public class AiPlayer<TCard>(PlayerAccountCard playerAccountCard, ILogger<AiPlayer<TCard>> logger) : Player<TCard>(logger)
    where TCard : Card
{
    public override PlayerAccountCard AccountCard => playerAccountCard;

    public ChannelReader<GameEventEnvelope>? CurrentGamesEvents { get; set; }

    protected override Task<int> PromptForIndexOfCardToPlay(
        uint prePromptEventId,
        Cards<TCard> cards,
        List<CardSelectionRule<TCard>> cardSelectionRules,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count));
    }

    protected override Task<List<int>> PromptForIndexesOfCardsToPlay(
        uint prePromptEventId,
        Cards<TCard> cards,
        List<CardComboSelectionRule<TCard>> cardComboSelectionRules,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<List<int>>(
        [
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count),
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count),
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count),
        ]);
    }

    protected override Task CardSelectedWasNotValid(Cards<TCard> cards, int iCardSelected, List<string> rulesFailed, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override Task CardsSelectedWereNotValid(Cards<TCard> cards, List<int> iCardsSelected, List<string> rulesFailed, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}