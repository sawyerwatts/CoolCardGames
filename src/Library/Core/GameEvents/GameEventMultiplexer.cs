using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.GameEvents;

public interface IEventMultiplexer
{
    GameEventHandler Handle { get; }
}

public interface IGameEventMultiplexerFactory
{
    IEventMultiplexer Make(IEnumerable<GameEventHandler> gameEventHandlers);
}

public class GameEventMultiplexerFactory(ILogger<EventMultiplexer> logger) : IGameEventMultiplexerFactory
{
    public IEventMultiplexer Make(IEnumerable<GameEventHandler> gameEventHandlers) =>
        new EventMultiplexer(gameEventHandlers, logger);
}

public class EventMultiplexer : IEventMultiplexer
{
    private readonly ILogger<EventMultiplexer> _logger;
    private event GameEventHandler OnGameEvent = null!;

    public EventMultiplexer(IEnumerable<GameEventHandler> consumers, ILogger<EventMultiplexer> logger)
    {
        foreach (GameEventHandler consumer in consumers)
            OnGameEvent += consumer;
        if (OnGameEvent is null)
            throw new ArgumentException($"{nameof(consumers)} needs at least one element");
        _logger = logger;
    }

    public GameEventHandler Handle => Run;

    private void Run(GameEvent gameEvent)
    {
        _logger.LogInformation("Multiplexing game event {GameEvent}", gameEvent);
        OnGameEvent(gameEvent);
    }
}
