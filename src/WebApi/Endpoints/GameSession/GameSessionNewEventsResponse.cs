using CoolCardGames.Library.Core.GameEventTypes;

namespace CoolCardGames.WebApi.Endpoints.GameSession;

public class GameSessionNewEventsResponse
{
    public IEnumerable<GameEvent> NewEvents { get; set; } = [];
    public string LastEventsId { get; set; } = "";
}