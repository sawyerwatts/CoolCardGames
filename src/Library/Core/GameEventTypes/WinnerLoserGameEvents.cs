using CoolCardGames.Library.Core.Players;

namespace CoolCardGames.Library.Core.GameEventTypes;

public abstract partial record GameEvent
{
    public record Winner(AccountCard AccountCard) : GameEvent($"{AccountCard} won!");

    public record Loser(AccountCard AccountCard) : GameEvent($"{AccountCard} lost");
}