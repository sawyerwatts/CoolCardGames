namespace CoolCardGames.Library.Core.State;

public class GameState<TPlayerState>(int numPlayers, Func<TPlayerState> makePlayerState)
    where TPlayerState : PlayerState
{
    public IReadOnlyList<TPlayerState> Players { get; set; } = numPlayers < 1
        ? throw new ArgumentException($"Must create a game state with at least one player, not {numPlayers} players")
        : Enumerable.Range(0, numPlayers)
            .Select(_ => makePlayerState())
            .ToList()
            .AsReadOnly();
}