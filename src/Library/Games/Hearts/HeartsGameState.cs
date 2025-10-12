using CoolCardGames.Library.Core.State;

namespace CoolCardGames.Library.Games.Hearts;

public class HeartsGameState : GameState<HeartsCard, HeartsPlayerState>
{
    public bool IsHeartsBroken { get; set; } = false;
}