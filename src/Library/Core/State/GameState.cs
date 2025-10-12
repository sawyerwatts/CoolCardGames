namespace CoolCardGames.Library.Core.State;

public class GameState<TCard, TPlayerState>
    where TCard : Card
    where TPlayerState : PlayerState<TCard>
{
    public IReadOnlyList<TPlayerState> Players { get; set; } = new List<TPlayerState>().AsReadOnly();
}