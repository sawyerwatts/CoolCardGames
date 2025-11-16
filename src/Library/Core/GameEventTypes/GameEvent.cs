namespace CoolCardGames.Library.Core.GameEventTypes;

/// <summary>
/// This type contains implementations that represent game events, like a card being played.
/// </summary>
/// <remarks>
/// These are intended primarily for UI notifications, but they can definitely be used for
/// other things like event-based game implementations or card-counting functionality.
/// </remarks>
public abstract partial record GameEvent(string Summary);
