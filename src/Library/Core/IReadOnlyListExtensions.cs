using GameEvent = CoolCardGames.Library.Core.GameEvents.GameEvent;

namespace CoolCardGames.Library.Core;

public static class IReadOnlyListExtensions
{
    /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})"/>
    public static int FindIndex<T>(this IReadOnlyList<T> ts, Predicate<T> match)
    {
        ArgumentNullException.ThrowIfNull(match, nameof(match));
        for (int i = 0; i < ts.Count; i++)
        {
            var t = ts[i];
            if (match(t))
                return i;
        }

        return -1;
    }

    public static void NotifyAll<TCard, TPlayerState, TGameState>(this IReadOnlyList<Player<TCard, TPlayerState, TGameState>> players, GameEvent gameEvent)
        where TCard : Card
        where TPlayerState : PlayerState<TCard>
        where TGameState : GameState<TCard, TPlayerState>
    {
        foreach (var player in players)
            player.Notify(gameEvent);
    }
}