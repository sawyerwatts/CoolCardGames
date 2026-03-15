namespace CoolCardGames.Library.Games.Hearts;

public static class HeartsGameRules
{
    public static readonly CardSelectionRule<HeartsCard> FirstTrickOpeningCardMustBeTwoOfClubs = new(
        Description: "The first trick must be opened with the two of clubs",
        ValidateCard: (hand, iCardToPlay) => hand[iCardToPlay].Value is TwoOfClubs);

    public static readonly CardSelectionRule<HeartsCard> FirstTrickCannotHavePoints = new(
        Description: "The first trick may not have point cards played",
        ValidateCard: (hand, iCardToPlay) => hand[iCardToPlay].Points == 0);

    public static CardSelectionRule<HeartsCard> HeartsCanOnlyBeLeadOnceBroken(bool isHeartsBroken)
    {
        return new CardSelectionRule<HeartsCard>(
            Description: "Hearts can only be lead once hearts is broken",
            ValidateCard: (hand, iCardToPlay) =>
            {
                var cardToPlay = hand[iCardToPlay];
                if (cardToPlay.Value.Suit is not Suit.Hearts)
                    return true;
                return isHeartsBroken;
            });
    }
}