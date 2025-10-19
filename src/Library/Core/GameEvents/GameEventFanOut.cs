using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.GameEvents;

public interface IGameEventFanOut
{
    GameEventHandler Handle { get; }
}

public interface IGameEventFanOutFactory
{
    IGameEventFanOut Make(IEnumerable<GameEventHandler> gameEventHandlers);
}

public class GameEventFanOutFactory(ILogger<GameEventFanOut> logger) : IGameEventFanOutFactory
{
    public IGameEventFanOut Make(IEnumerable<GameEventHandler> gameEventHandlers) =>
        new GameEventFanOut(gameEventHandlers, logger);
}

public class GameEventFanOut : IGameEventFanOut
{
    private readonly ILogger<GameEventFanOut> _logger;
    private event GameEventHandler OnGameEvent = null!;

    public GameEventFanOut(IEnumerable<GameEventHandler> consumers, ILogger<GameEventFanOut> logger)
    {
        foreach (GameEventHandler consumer in consumers)
            OnGameEvent += consumer;
        if (OnGameEvent is null)
            throw new ArgumentException($"{nameof(consumers)} needs at least one element");
        _logger = logger;
    }

    public GameEventHandler Handle => PublishGameEvent;

    private void PublishGameEvent(GameEvent gameEvent)
    {
        _logger.LogInformation("Publishing game event {GameEvent}", gameEvent);
        OnGameEvent.Invoke(gameEvent);
    }
}
