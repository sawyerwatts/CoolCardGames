namespace CoolCardGames.Library.Games.Hearts;

public class HeartsGameState() : GameState<HeartsPlayerState>(HeartsGame.NumPlayers, () => new HeartsPlayerState())
{
    public bool IsFirstTrick { get; set; } = true;
    public int IndexTrickStartPlayer { get; set; } = 0;
    public bool IsHeartsBroken { get; set; } = false;
}