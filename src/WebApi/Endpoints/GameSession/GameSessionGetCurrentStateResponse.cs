using CoolCardGames.Library.Core.CardTypes;
using CoolCardGames.Library.Core.GameEventTypes;

namespace CoolCardGames.WebApi.Endpoints.GameSession;

public class GameSessionGetCurrentStateResponse
{
    public IEnumerable<Card> Cards { get; set; } = [];
    public string LastEventId => NewGameEvents.Count > 0 ? NewGameEvents[^1].Id.ToString() : "";
    public List<GameEventEnvelope> NewGameEvents { get; set; } = [];
    public IEnumerable<string>? IfNotNullSelectCardFollowingTheseRules { get; set; }
    public IEnumerable<string>? IfNotNullSelectCardComboFollowingTheseRules { get; set; }

    // TODO: include the public state info (num cards other players have, scores, etc)
}