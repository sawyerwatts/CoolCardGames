namespace CoolCardGames.WebApi.GameTypes;

public struct GameTypesGetResponse()
{
    public IEnumerable<GameType> GameTypes { get; set; } = [];
}