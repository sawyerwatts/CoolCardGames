using System.Diagnostics;

using CoolCardGames.Library.Core.Players;

namespace CoolCardGames.Library.Games.Hearts;

public abstract record HeartsGameEvent(string Summary) : GameEvent(Summary)
{
    public record HoldEmRound() : HeartsGameEvent("Hold 'em round! No passing")
    {
        public static readonly HoldEmRound Singleton = new();
    }

    public record GetReadyToPass(PassDirection PassDirection) : HeartsGameEvent(
        PassDirection switch
        {
            PassDirection.Left or PassDirection.Right or PassDirection.Across => $"Get ready to pass three cards {PassDirection}",
            PassDirection.Hold => throw new ArgumentException("Cannot pass cards when holding"),
            _ => throw new UnreachableException($"Unknown {nameof(PassDirection)} given: {PassDirection}")
        });

    public record CardsPassed(PassDirection PassDirection) : HeartsGameEvent(
        PassDirection switch
        {
            PassDirection.Left or PassDirection.Right or PassDirection.Across  => $"Passed three cards {PassDirection}",
            PassDirection.Hold => throw new ArgumentException("Could not have passed cards when holding"),
            _ => throw new UnreachableException($"Unknown {nameof(PassDirection)} given: {PassDirection}")
        });

    public record ShotTheMoon(AccountCard AccountCard) : HeartsGameEvent($"{AccountCard} shot the moon!");

    public record TrickScored(AccountCard AccountCard, int RoundPoints, int TotalPoints)
        : HeartsGameEvent($"{AccountCard} scored {RoundPoints} point(s) this round and has a total of {TotalPoints} point(s)");

    public record HeartsHaveBeenBroken(AccountCard AccountCard, HeartsCard HeartsCard)
        : HeartsGameEvent($"Hearts have been broken by {AccountCard} with {HeartsCard}");
}