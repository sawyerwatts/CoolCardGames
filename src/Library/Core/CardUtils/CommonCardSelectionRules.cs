namespace CoolCardGames.Library.Core.CardUtils;

public static class CommonCardSelectionRules
{
    /// <summary>
    /// This returns a rule that will this will return true if the played card's suit is following the
    /// given suit OR if none given cards' suits can follow suit.
    /// </summary>
    /// <example>
    /// - If the suit to follow is hearts and there is a hearts in the given cards, then the chosen
    /// card must be hearts.
    /// <br /> - If the suit to follow is hearts and there are no hearts in the given cards, then
    /// the chosen card can have any suit.
    /// </example>
    public static CardSelectionRule<TCard> IsSuitFollowedIfPossible<TCard>(Suit suitToFollow)
        where TCard : Card
    {
        return new CardSelectionRule<TCard>(
            Description: $"If possible, play a {suitToFollow}",
            ValidateCard: (hand, iCardToPlay) =>
            {
                var cardToPlay = hand[iCardToPlay];
                if (cardToPlay.Value.Suit == suitToFollow)
                    return true;

                var canFollowSuit = hand.Any(card => card.Value.Suit == suitToFollow);
                return !canFollowSuit;
            });
    }

    public static CardComboSelectionRule<TCard> LimitNumberOfCardsSelected<TCard>(int exactNumberOfCardsToPlay)
        where TCard : Card
    {
        return LimitNumberOfCardsSelected<TCard>(exactNumberOfCardsToPlay, exactNumberOfCardsToPlay);
    }

    public static CardComboSelectionRule<TCard> LimitNumberOfCardsSelected<TCard>(int minCardsToPlay, int maxCardsToPlay)
        where TCard : Card
    {
        if (minCardsToPlay < 0)
            throw new ArgumentException($"{nameof(minCardsToPlay)} is less than 0");
        if (minCardsToPlay > maxCardsToPlay)
            throw new ArgumentException($"{nameof(minCardsToPlay)} is higher than {nameof(maxCardsToPlay)}");
        return new CardComboSelectionRule<TCard>(
            Description: minCardsToPlay == maxCardsToPlay
                ? $"Select exactly {minCardsToPlay} card(s)"
                : $"Select between {minCardsToPlay} and {maxCardsToPlay} cards (inclusive)",
            ValidateCards: (_, iCardsToPlay) =>
            {
                if (iCardsToPlay.Count < minCardsToPlay)
                    return false;
                if (iCardsToPlay.Count > maxCardsToPlay)
                    return false;
                return true;
            });
    }
}