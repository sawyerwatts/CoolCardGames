namespace CoolCardGames.Library.Core.GameEventTypes;

public readonly record struct GameEventEnvelope(GameEvent GameEvent, uint Id)
{
    public GameEventEnvelope()
        : this(null!, 0)
    {
        throw new NotSupportedException("The default constructor is not supported");
    }
}