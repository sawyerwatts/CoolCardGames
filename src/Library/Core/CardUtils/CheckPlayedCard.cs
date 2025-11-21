namespace CoolCardGames.Library.Core.CardUtils;

public static class CheckPlayedCard
{
    /// <summary>
    /// If the suit cannot be followed, this will return true.
    /// </summary>
    public static bool IsSuitFollowedIfPossible<TCard>(Suit suitToFollow, Cards<TCard> playerHand,
        int iPlayerCardToPlay, Suit? trumpSuit = null)
        where TCard : Card
    {
        var cardToPlay = playerHand[iPlayerCardToPlay];
        if (cardToPlay.Value.Suit == suitToFollow)
            return true;

        if (trumpSuit is not null && cardToPlay.Value.Suit == trumpSuit)
            return true;

        var canFollowSuit = playerHand.Any(card => card.Value.Suit == suitToFollow);
        return !canFollowSuit;
    }
}