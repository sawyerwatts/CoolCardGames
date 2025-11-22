using CoolCardGames.Library.Core.GameEventTypes;
using CoolCardGames.Library.Core.GameEventUtils;

namespace CoolCardGames.XUnitTests;

public class ListGameEventPublisher : IGameEventPublisher
{
    public List<GameEventEnvelope> Events { get; } = [];

    public ValueTask<GameEventEnvelope> Publish(GameEvent gameEvent, CancellationToken cancellationToken)
    {
        var envelope = new GameEventEnvelope(gameEvent, 1);
        Events.Add(envelope);
        return ValueTask.FromResult(envelope);
    }
}