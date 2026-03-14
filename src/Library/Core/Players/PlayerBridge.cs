using System.Threading.Channels;

namespace CoolCardGames.Library.Core.Players;

/// <summary>
/// This class exists so that if a player disconnects, they can be seamlessly swapped to an AI.
/// </summary>
// TODO: actually implement the swapping
public class PlayerBridge<TCard>(IPlayer<TCard> player /*, AiPlayerFactory aiFactory */)
    : IPlayer<TCard>
    where TCard : Card
{
    public PlayerAccountCard AccountCard => player.AccountCard;

    public Disposable JoinGame(ChannelReader<GameEventEnvelope> currGamesEvents, IGameEventPublisher currGameEventPublisher)
    {
        return player.JoinGame(currGamesEvents, currGameEventPublisher);
    }

    public Task<TCard> PromptForValidCardAndPlay(Cards<TCard> cards, Func<Cards<TCard>, int, bool> validateChosenCard, CancellationToken cancellationToken, bool reveal = true)
    {
        return player.PromptForValidCardAndPlay(cards, validateChosenCard, cancellationToken, reveal);
    }

    public Task<Cards<TCard>> PromptForValidCardsAndPlay(Cards<TCard> cards, Func<Cards<TCard>, List<int>, bool> validateChosenCards, CancellationToken cancellationToken, bool reveal = true)
    {
        return player.PromptForValidCardsAndPlay(cards, validateChosenCards, cancellationToken, reveal);
    }
}