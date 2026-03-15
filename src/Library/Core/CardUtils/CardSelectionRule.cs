namespace CoolCardGames.Library.Core.CardUtils;

/// <param name="iCardToPlay">
/// This can be assumed to be a valid index within <paramref name="cards"/>.
/// </param>
public delegate bool CardSelectionValidation<TCard>(
    Cards<TCard> cards,
    int iCardToPlay)
    where TCard : Card;

public readonly record struct CardSelectionRule<TCard>(
    string Description,
    CardSelectionValidation<TCard> ValidateCard)
    where TCard : Card;

////////////////////////////////////////////////////////////////////////////////

/// <param name="iCardsToPlay">
/// These can be assumed to be unique, valid indexes within <paramref name="cards"/>.
/// </param>
public delegate bool CardComboSelectionValidation<TCard>(
    Cards<TCard> cards,
    List<int> iCardsToPlay)
    where TCard : Card;

public readonly record struct CardComboSelectionRule<TCard>(
    string Description,
    CardComboSelectionValidation<TCard> ValidateCards)
    where TCard : Card;
