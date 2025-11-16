namespace CoolCardGames.Library.Core.GameEventTypes;

/// <summary>
/// </summary>
/// <param name="Id">
/// This should be treated as an opaque string: equality can be performed, but not comparisons
/// (less-than or greater-than).
/// </param>
public readonly record struct GameEventEnvelope(GameEvent GameEvent, string Id)
{
    public GameEventEnvelope()
        : this(null!, null!)
    {
        throw new NotSupportedException("The default constructor is not supported");
    }
}