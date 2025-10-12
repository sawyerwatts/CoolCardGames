using CoolCardGames.Library.Core.State;

namespace CoolCardGames.Library.Games.Hearts;

public class HeartsPlayerState : PlayerState<HeartsCard>
{
    public List<Cards<HeartsCard>> TricksTaken { get; set; } = [];

    public int Score { get; set; } = 0;
}