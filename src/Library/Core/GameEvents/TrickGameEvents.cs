using CoolCardGames.Library.Core.Actors;

namespace CoolCardGames.Library.Core.GameEvents;

public abstract partial record GameEvent
{
    public record CardAddedToTrick(AccountCard Actor, Card Card) : GameEvent($"The card {Card} was added to the trick by {Actor}");

    public record TrickTaken(AccountCard Actor, Card Card) : GameEvent($"{Actor} took the trick with card {Card}");
}
