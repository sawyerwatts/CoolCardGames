using System.Text.Json.Serialization;

using CoolCardGames.Library.Core.GameEventTypes;

namespace CoolCardGames.WebApi.Endpoints.GameSession;

public class GameSessionGetCurrentStateResponse
{
    public IEnumerable<CardDto> Cards { get; set; } = [];
    public IEnumerable<string>? IfNotNullSelectCardFollowingTheseRules { get; set; }
    public IEnumerable<string>? IfNotNullSelectCardComboFollowingTheseRules { get; set; }

    [JsonIgnore] public List<GameEventEnvelope> NewGameEventEnvelopes { get; set; } = [];
    public IEnumerable<object> NewGameEvents => NewGameEventEnvelopes.Select(envelope => envelope.GameEvent);
}