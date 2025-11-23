using System.Threading.Channels;

namespace CoolCardGames.Library.Core.Players;

/// <summary>
/// This class exists so that if a player disconnects, they can be seamlessly swapped to an AI.
/// </summary>
public class PlayerBridge<TCard>(IPlayer<TCard> player /*, AiPlayerFactory aiFactory */)
    : IPlayer<TCard>
    where TCard : Card
{
    public PlayerAccountCard AccountCard => player.AccountCard;

    public ChannelReader<GameEventEnvelope>? CurrentGamesEvents { get; set; }

    public Task<int> PromptForIndexOfCardToPlay(uint prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        return player.PromptForIndexOfCardToPlay(prePromptEventId, cards, cancellationToken);
    }

    public Task<List<int>> PromptForIndexesOfCardsToPlay(uint prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        return player.PromptForIndexesOfCardsToPlay(prePromptEventId, cards, cancellationToken);
    }
}