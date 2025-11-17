namespace CoolCardGames.Library.Core.State;

public class GameState<TCard, TPlayerState>(int numPlayers, Func<TPlayerState> makePlayerState)
    where TCard : Card
    where TPlayerState : PlayerState<TCard>
{
    public IReadOnlyList<TPlayerState> Players { get; set; } = numPlayers < 1
        ? throw new ArgumentException($"Must create a game state with at least one player, not {numPlayers} players")
        : Enumerable.Range(0, numPlayers)
            .Select(_ => makePlayerState())
            .ToList()
            .AsReadOnly();
}