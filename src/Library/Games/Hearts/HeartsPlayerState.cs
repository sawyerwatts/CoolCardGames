namespace CoolCardGames.Library.Games.Hearts;

public class HeartsPlayerState : PlayerState
{
    public List<Cards> TricksTaken { get; set; } = [];

    public int Score { get; set; } = 0;
}