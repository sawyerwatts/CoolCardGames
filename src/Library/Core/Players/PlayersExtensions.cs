namespace CoolCardGames.Library.Core.Players;

public static class PlayersExtensions
{
    public static void NotifyAll<TCard, TPlayerState, TGameState>(this IReadOnlyList<Player<TCard, TPlayerState, TGameState>> players, GameEvent gameEvent)
        where TCard : Card
        where TPlayerState : PlayerState<TCard>
        where TGameState : GameState<TCard, TPlayerState>
    {
        foreach (var player in players)
            player.Notify(gameEvent);
    }
}