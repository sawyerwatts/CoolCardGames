using System.Text.Json.Serialization;

using CoolCardGames.Library.Core.CardTypes;
using CoolCardGames.Library.Core.GameEventTypes;

namespace CoolCardGames.WebApi.Endpoints.GameSession;

public class GameSessionGetCurrentStateResponse
{
    public IEnumerable<Card> Cards { get; set; } = [];
    public string LastEventId => NewGameEventEnvelopes.Count > 0 ? NewGameEventEnvelopes[^1].Id.ToString() : "";
    public IEnumerable<object> NewGameEvents => NewGameEventEnvelopes.Select(envelope => envelope.GameEvent);
    public IEnumerable<string>? IfNotNullSelectCardFollowingTheseRules { get; set; }
    public IEnumerable<string>? IfNotNullSelectCardComboFollowingTheseRules { get; set; }

    [JsonIgnore] public List<GameEventEnvelope> NewGameEventEnvelopes { get; set; } = [];

    // TODO: include the public state info (num cards other players have, scores, etc)
}