using CoolCardGames.Library.Core.Players;

namespace CoolCardGames.Library.Core.GameEventTypes;

public abstract partial record GameEvent
{
    public record ActorHasTheAction(AccountCard Actor) : GameEvent($"{Actor} has the action");

    public record ActorPassed(AccountCard Actor) : GameEvent($"{Actor} has passed");

    public record ActorPlayedCard<TCard>(AccountCard Actor, TCard Card) : GameEvent($"{Actor} played card {Card}") where TCard : Card;

    public record ActorPlayedCards<TCard>(AccountCard Actor, Cards<TCard> Cards) : GameEvent($"{Actor} played card(s): {SerializeCards(Cards)}") where TCard : Card;

    public record ActorTookTrickWithCard<TCard>(AccountCard Actor, TCard Card) : GameEvent($"{Actor} took the trick with card {Card}") where TCard : Card;

    public record ActorTookTrickWithCards<TCard>(AccountCard Actor, Cards<TCard> Cards) : GameEvent($"{Actor} took the trick with card(s): {SerializeCards(Cards)}") where TCard : Card;
}