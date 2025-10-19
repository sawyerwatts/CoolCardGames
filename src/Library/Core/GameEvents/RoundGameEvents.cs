namespace CoolCardGames.Library.Core.GameEvents;

public abstract partial record GameEvent
{
    public record SettingUpNewRound() : GameEvent("Setting up a new round")
    {
        public static readonly SettingUpNewRound Singleton = new();
    }

    public record BeginningNewRound() : GameEvent("Beginning a new round")
    {
        public static readonly BeginningNewRound Singleton = new();
    }

    public record ScoringRound() : GameEvent("Scoring round")
    {
        public static readonly ScoringRound Singleton = new();
    }
}