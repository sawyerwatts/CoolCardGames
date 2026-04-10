namespace CoolCardGames.WebApi.Endpoints.GameSession;

public class GameSessionGetResponse
{
    public IEnumerable<GameSessionGetResponseItem> Items { get; set; } = [];
}