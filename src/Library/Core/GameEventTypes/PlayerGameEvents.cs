using CoolCardGames.Library.Core.Players;

namespace CoolCardGames.Library.Core.GameEventTypes;

public abstract partial record GameEvent
{
    public record HandGiven(PlayerAccountCard Recipient, int NumCardsInHand) : GameEvent($"{Recipient} was given a hand with {NumCardsInHand} cards");

    public record PlayerHasTheAction(PlayerAccountCard PlayerCard) : GameEvent($"{PlayerCard} has the action");

    public record PlayerPassed(PlayerAccountCard PlayerCard) : GameEvent($"{PlayerCard} has passed");

    public record PlayerPlayedCard(PlayerAccountCard PlayerCard, Card Card) : GameEvent($"{PlayerCard} played: {Card}");

    public record PlayerPlayedCards(PlayerAccountCard PlayerCard, Cards Cards) : GameEvent($"{PlayerCard} played: {SerializeCards(Cards)}");

    public record PlayerPlayedHiddenCard(PlayerAccountCard PlayerCard) : GameEvent($"{PlayerCard} played: a hidden card");

    public record PlayerPlayedHiddenCards(PlayerAccountCard PlayerCard, int numCards) : GameEvent($"{PlayerCard} played: {numCards} hidden card(s)");

    public record PlayerReceivedHiddenCard(PlayerAccountCard PlayerCard) : GameEvent($"{PlayerCard} received: a hidden card");

    public record PlayerReceivedHiddenCards(PlayerAccountCard PlayerCard, int numCards) : GameEvent($"{PlayerCard} received: {numCards} hidden card(s)");

    public record PlayerTookTrickWithCard(PlayerAccountCard PlayerCard, Card Card) : GameEvent($"{PlayerCard} took the trick with: {Card}");

    public record PlayerTookTrickWithCards(PlayerAccountCard PlayerCard, Cards Cards) : GameEvent($"{PlayerCard} took the trick with: {SerializeCards(Cards)}");

    public record PlayerAtOrExceededMaxPoints(PlayerAccountCard PlayerAccountCard, int Points, int MaxPoints)
        : GameEvent($"{PlayerAccountCard} has {Points} points, which is at or over max allowed points of {MaxPoints}");
}