namespace CoolCardGames.WebApi.Endpoints.GameSession;

public class GameSessionPlayCardsResponse
{
    /// <summary>
    /// When true, this will be the only value initialized.
    /// </summary>
    public bool AcceptedCardsPlayed { get; set; }

    public IEnumerable<string>? RulesFailed { get; set; }
    public List<int>? IndexesOfCardsAttempted { get; set; }
    public IEnumerable<CardDto>? AllCards { get; set; }
}