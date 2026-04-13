namespace CoolCardGames.WebApi.Endpoints.GameSession;

public class GameSessionPlayCardResponse
{
    /// <summary>
    /// When true, this will be the only value initialized.
    /// </summary>
    public bool AcceptedCardPlayed { get; set; }

    public IEnumerable<string>? RulesFailed { get; set; }
    public int? IndexOfCardAttempted { get; set; }
    public IEnumerable<CardDto>? AllCards { get; set; }
}