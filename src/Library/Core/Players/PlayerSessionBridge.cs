using System.Threading.Channels;

namespace CoolCardGames.Library.Core.Players;

// TODO: somehow swap out impl to ai player session
//       if prompt impl keeps throwing, swap?
//       healthcheck on PlayerSession?

/// <summary>
/// This class exists so that if a player disconnects, they can be seamlessly swapped to an AI.
/// </summary>
/// <param name="playerSession"></param>
/// <param name="aiFactory"></param>
/// <typeparam name="TCard"></typeparam>
public class PlayerSessionBridge<TCard>(IPlayerSession<TCard> playerSession, AiPlayerFactory aiFactory)
    : IPlayerSession<TCard>
    where TCard : Card
{
    public AccountCard AccountCard => playerSession.AccountCard;

    public ChannelReader<GameEvent>? CurrentGamesEvents { get; set; }

    public Task<int> PromptForIndexOfCardToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        return playerSession.PromptForIndexOfCardToPlay(cards, cancellationToken);
    }

    public Task<List<int>> PromptForIndexesOfCardsToPlay(Cards<TCard> cards, CancellationToken cancellationToken)
    {
        return playerSession.PromptForIndexesOfCardsToPlay(cards, cancellationToken);
    }
}