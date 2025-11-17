using CoolCardGames.Library.Core.Players;

namespace CoolCardGames.Library.Core.GameEventTypes;

public abstract partial record GameEvent
{
    public record PlayerHasTheAction(PlayerAccountCard PlayerCard) : GameEvent($"{PlayerCard} has the action");

    public record PlayerPassed(PlayerAccountCard PlayerCard) : GameEvent($"{PlayerCard} has passed");

    public record PlayerPlayedCard<TCard>(PlayerAccountCard PlayerCard, TCard Card) : GameEvent($"{PlayerCard} played card {Card}") where TCard : Card;

    public record PlayerPlayedCards<TCard>(PlayerAccountCard PlayerCard, Cards<TCard> Cards) : GameEvent($"{PlayerCard} played card(s): {SerializeCards(Cards)}") where TCard : Card;

    public record PlayerTookTrickWithCard<TCard>(PlayerAccountCard PlayerCard, TCard Card) : GameEvent($"{PlayerCard} took the trick with card {Card}") where TCard : Card;

    public record PlayerTookTrickWithCards<TCard>(PlayerAccountCard PlayerCard, Cards<TCard> Cards) : GameEvent($"{PlayerCard} took the trick with card(s): {SerializeCards(Cards)}") where TCard : Card;
}