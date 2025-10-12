using System.ComponentModel.DataAnnotations;

namespace CoolCardGames.Library.Games.HeartsGame;

public class HeartsSettings
{
    /// <summary>
    /// The game will end when a round is completed and someone has at least this many points.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int EndOfGamePoints { get; set; } = 100;
}