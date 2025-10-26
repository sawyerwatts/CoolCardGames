using CoolCardGames.Library.Core.Actors;

namespace CoolCardGames.Library.Core.GameEvents;

public abstract partial record GameEvent
{
    public record Winner(AccountCard AccountCard) : GameEvent($"{AccountCard} won!");

    public record Loser(AccountCard AccountCard) : GameEvent($"{AccountCard} lost");
}