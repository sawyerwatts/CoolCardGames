using System.Threading.Channels;

namespace CoolCardGames.Library.Core.Players;

public class AiPlayer<TCard>(PlayerAccountCard playerAccountCard) : IPlayer<TCard>
    where TCard : Card
{
    public PlayerAccountCard PlayerAccountCard => playerAccountCard;

    public ChannelReader<GameEventEnvelope>? CurrentGamesEvents { get; set; }

    public Task<int> PromptForIndexOfCardToPlay(string prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<int>> PromptForIndexesOfCardsToPlay(string prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}