using System.Security.Cryptography;
using System.Threading.Channels;

namespace CoolCardGames.Library.Core.Players;

public class AiPlayer<TCard>(PlayerAccountCard playerAccountCard) : IPlayer<TCard>
    where TCard : Card
{
    public PlayerAccountCard PlayerAccountCard => playerAccountCard;

    public ChannelReader<GameEventEnvelope>? CurrentGamesEvents { get; set; }

    public Task<int> PromptForIndexOfCardToPlay(string prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count));
    }

    public Task<List<int>> PromptForIndexesOfCardsToPlay(string prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        return Task.FromResult<List<int>>(
        [
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count),
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count),
            RandomNumberGenerator.GetInt32(fromInclusive: 0, toExclusive: cards.Count),
        ]);
    }
}