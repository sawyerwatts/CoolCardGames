using System.Diagnostics;

namespace CoolCardGames.Library.Games.Hearts;

public abstract partial record GameEvent
{
    public record HeartsHoldEmRound() : Core.GameEvents.GameEvent("Hold 'em round! No passing")
    {
        public static readonly HeartsHoldEmRound Singleton = new();
    }

    public record HeartsGetReadyToPass(PassDirection PassDirection) : Core.GameEvents.GameEvent(
        PassDirection switch
        {
            PassDirection.Left or PassDirection.Right or PassDirection.Across => $"Get ready to pass three cards {PassDirection}",
            PassDirection.Hold => throw new ArgumentException("Cannot pass cards when holding"),
            _ => throw new UnreachableException($"Unknown {nameof(PassDirection)} given: {PassDirection}")
        });

    public record HeartsCardsPassed(PassDirection PassDirection) : Core.GameEvents.GameEvent(
        PassDirection switch
        {
            PassDirection.Left or PassDirection.Right or PassDirection.Across  => $"Passed three cards {PassDirection}",
            PassDirection.Hold => throw new ArgumentException("Could not have passed cards when holding"),
            _ => throw new UnreachableException($"Unknown {nameof(PassDirection)} given: {PassDirection}")
        });

    // TODO: scores updated

    // TODO: extend CardAddedToTrick for HeartsBroken?
}