namespace CoolCardGames.Library.Core.CardUtils;

public delegate bool CardSelectionValidation<TCard>(
    Cards<TCard> cards,
    int iCardToPlay)
    where TCard : Card;

public readonly record struct CardSelectionRule<TCard>(
    string Description,
    CardSelectionValidation<TCard> ValidateCard)
    where TCard : Card;

////////////////////////////////////////////////////////////////////////////////

public delegate bool CardComboSelectionValidation<TCard>(
    Cards<TCard> cards,
    List<int> iCardsToPlay)
    where TCard : Card;

public readonly record struct CardComboSelectionRule<TCard>(
    string Description,
    CardComboSelectionValidation<TCard> ValidateCards)
    where TCard : Card;
