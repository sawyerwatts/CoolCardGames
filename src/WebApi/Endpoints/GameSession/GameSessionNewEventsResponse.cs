using CoolCardGames.WebApi.GameTypeDtos.GameEvents;

namespace CoolCardGames.WebApi.Endpoints.GameSession;

public class GameSessionNewEventsResponse
{
    public IEnumerable<IGameEventDto> NewEvents { get; set; } = [];
    public string LastEventsId { get; set; } = "";
}