using CoolCardGames.WebApi.GameTypeDtos.GameEvents;

namespace CoolCardGames.WebApi.Endpoints.GameSessions;

public class GameSessionsNewEventsResponse
{
    public IEnumerable<IGameEventDto> NewEvents { get; set; } = [];
    public string LastEventsId { get; set; } = "";
}