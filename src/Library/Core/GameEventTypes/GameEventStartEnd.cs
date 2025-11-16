namespace CoolCardGames.Library.Core.GameEventTypes;

public abstract partial record GameEvent
{
    public record GameStarted(string GameName) : GameEvent($"Started a game of {GameName}");

    public record GameEnded(string GameName, bool CompletedNormally) : GameEvent($"Ended a game of {GameName}; did it complete normally? {CompletedNormally}");
}