namespace CoolCardGames.WebApi.Endpoints.GameSession;

public class GameSessionPostResponse
{
    public string SessionId { get; set; } = "";
    public string GameType { get; set; } = "";
    public string OwningPlayerId { get; set; } = "";
}