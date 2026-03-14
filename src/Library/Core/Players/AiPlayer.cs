using System.Security.Cryptography;
using System.Threading.Channels;

namespace CoolCardGames.Library.Core.Players;

public class AiPlayer<TCard>(PlayerAccountCard playerAccountCard) : Player<TCard>
    where TCard : Card
{
    public override PlayerAccountCard AccountCard => playerAccountCard;

    public ChannelReader<GameEventEnvelope>? CurrentGamesEvents { get; set; }

    protected override Task<int> PromptForIndexOfCardToPlay(uint prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count));
    }

    protected override Task<List<int>> PromptForIndexesOfCardsToPlay(uint prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        return Task.FromResult<List<int>>(
        [
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count),
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count),
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count),
        ]);
    }
}