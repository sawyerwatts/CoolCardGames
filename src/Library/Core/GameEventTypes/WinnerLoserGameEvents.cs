using CoolCardGames.Library.Core.Players;

namespace CoolCardGames.Library.Core.GameEventTypes;

public abstract partial record GameEvent
{
    public record Winner(PlayerAccountCard PlayerAccountCard) : GameEvent($"{PlayerAccountCard} won!");

    public record Loser(PlayerAccountCard PlayerAccountCard) : GameEvent($"{PlayerAccountCard} lost");
}