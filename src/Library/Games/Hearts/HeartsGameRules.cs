namespace CoolCardGames.Library.Games.Hearts;

public static class HeartsGameRules
{
    public static readonly CardSelectionRule<HeartsCard> FirstTrickOpeningCardMustBeTwoOfClubs = new(
        description: "The first trick must be opened with the two of clubs",
        validateCard: (hand, iCardToPlay) => hand[iCardToPlay].Value is TwoOfClubs);

    public static readonly CardSelectionRule<HeartsCard> FirstTrickCannotHavePoints = new(
        description: "The first trick may not have point cards played",
        validateCard: (hand, iCardToPlay) => hand[iCardToPlay].Points == 0);

    public static CardSelectionRule<HeartsCard> HeartsCanOnlyBeLeadOnceBroken(bool isHeartsBroken)
    {
        return new CardSelectionRule<HeartsCard>(
            description: "Hearts can only be lead once hearts is broken",
            validateCard: (hand, iCardToPlay) =>
            {
                var cardToPlay = hand[iCardToPlay];
                if (cardToPlay.Value.Suit is not Suit.Hearts)
                    return true;
                return isHeartsBroken;
            });
    }
}