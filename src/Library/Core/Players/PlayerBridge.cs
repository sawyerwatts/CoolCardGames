using System.Threading.Channels;

namespace CoolCardGames.Library.Core.Players;

// TODO: somehow swap out impl to ai player session
//       if prompt impl keeps throwing, swap?
//       healthcheck on PlayerSession?
//       timeouts on req prob too
// TODO: it'd be slick to refactor CliPlayer's sync logic here
//       maybe have players note what step they're on so this could tell if something's going haywire?

/// <summary>
/// This class exists so that if a player disconnects, they can be seamlessly swapped to an AI.
/// </summary>
public class PlayerBridge<TCard>(IPlayer<TCard> player /*, AiPlayerFactory aiFactory */)
    : IPlayer<TCard>
    where TCard : Card
{
    public PlayerAccountCard PlayerAccountCard => player.PlayerAccountCard;

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