using CoolCardGames.Library.Core.GameEventTypes;
using CoolCardGames.Library.Core.GameEventUtils;

namespace CoolCardGames.XUnitTests;

public class ListGameEventPublisher : IGameEventPublisher
{
    public List<GameEvent> Events { get; } = [];

    public ValueTask Publish(GameEvent gameEvent, CancellationToken cancellationToken)
    {
        Events.Add(gameEvent);
        return ValueTask.CompletedTask;
    }
}