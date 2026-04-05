namespace CoolCardGames.WebApi.Endpoints.GameTypes;

public class GameTypesGetResponse
{
    public IEnumerable<GameType> GameTypes { get; set; } = [];
}