namespace CoolCardGames.WebApi.Endpoints.GameType;

public struct GameType()
{
    public string Name { get; set; } = "";
    public int MinPlayers { get; set; } = 0;
    public int MaxPlayers { get; set; } = 0;
    public string Description { get; set; } = "";
}