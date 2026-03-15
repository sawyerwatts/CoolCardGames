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

    public Task<TCard> PromptForValidCardAndPlay(Cards<TCard> cards, CardSelectionRule<TCard> cardSelectionRule, CancellationToken cancellationToken, bool reveal = true)
    {
        return player.PromptForValidCardAndPlay(cards, cardSelectionRule, cancellationToken, reveal);
    }

    public Task<TCard> PromptForValidCardAndPlay(Cards<TCard> cards, List<CardSelectionRule<TCard>> cardSelectionRules, CancellationToken cancellationToken, bool reveal = true)
    {
        return player.PromptForValidCardAndPlay(cards, cardSelectionRules, cancellationToken, reveal);
    }

    public Task<Cards<TCard>> PromptForValidCardsAndPlay(Cards<TCard> cards, CardComboSelectionRule<TCard> cardComboSelectionRule, CancellationToken cancellationToken, bool reveal = true)
    {
        return player.PromptForValidCardsAndPlay(cards, cardComboSelectionRule, cancellationToken, reveal);
    }

    public Task<Cards<TCard>> PromptForValidCardsAndPlay(Cards<TCard> cards, List<CardComboSelectionRule<TCard>> cardComboSelectionRules, CancellationToken cancellationToken, bool reveal = true)
    {
        return player.PromptForValidCardsAndPlay(cards, cardComboSelectionRules, cancellationToken, reveal);
    }
}