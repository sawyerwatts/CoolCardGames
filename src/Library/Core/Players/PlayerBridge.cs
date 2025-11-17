using System.Threading.Channels;

namespace CoolCardGames.Library.Core.Players;

// TODO: somehow swap out impl to ai player session
//       if prompt impl keeps throwing, swap?
//       healthcheck on PlayerSession?
//       timeouts on req prob too

/// <summary>
/// This class exists so that if a player disconnects, they can be seamlessly swapped to an AI.
/// </summary>
/// <param name="player"></param>
/// <param name="aiFactory"></param>
/// <typeparam name="TCard"></typeparam>
public class PlayerBridge<TCard>(IPlayer<TCard> player, AiPlayerFactory aiFactory)
    : IPlayer<TCard>
    where TCard : Card
{
    public PlayerAccountCard PlayerAccountCard => player.PlayerAccountCard;

    public ChannelReader<GameEventEnvelope>? CurrentGamesEvents { get; set; }

    public Task<int> PromptForIndexOfCardToPlay(string prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        return player.PromptForIndexOfCardToPlay(prePromptEventId, cards, cancellationToken);
    }

    public Task<List<int>> PromptForIndexesOfCardsToPlay(string prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        return player.PromptForIndexesOfCardsToPlay(prePromptEventId, cards, cancellationToken);
    }
}