namespace CoolCardGames.Library.Core;

public static class CheckPlayedCard
{
    /// <summary>
    /// If the suit cannot be followed, this will return true.
    /// </summary>
    public static bool IsSuitFollowedIfPossible<TCard>(Suit suitToFollow, Cards<TCard> playerHand,
        int iPlayerCardToPlay, Suit? bypassSuit = null)
        where TCard : Card
    {
        TCard cardToPlay = playerHand[iPlayerCardToPlay];
        if (cardToPlay.Value.Suit == suitToFollow)
            return true;

        if (bypassSuit is not null && cardToPlay.Value.Suit == bypassSuit)
            return true;

        bool canFollowSuit = playerHand.Any(card => card.Value.Suit == suitToFollow);
        return !canFollowSuit;
    }
}