namespace CoolCardGames.Library.Core.CardUtils;

public delegate bool CardSelectionValidation<TCard>(
    Cards<TCard> cards,
    int iCardToPlay)
    where TCard : Card;

public readonly struct CardSelectionRule<TCard>(
    string description,
    CardSelectionValidation<TCard> validateCard)
    where TCard : Card
{
    public CardSelectionRule() : this(null!, null!) => throw new NotSupportedException();

    public string Description { get; } = description;

    /// <remarks>
    /// This will pre-validate <paramref name="iCardToPlay"/> before executing the injected <see cref="CardSelectionValidation{TCard}"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throw when <paramref name="iCardToPlay"/> is out of range of <paramref name="cards"/>.
    /// </exception>
    public bool ValidateCard(Cards<TCard> cards, int iCardToPlay)
    {
        if (iCardToPlay < 0 || iCardToPlay >= cards.Count)
            throw new ArgumentOutOfRangeException(nameof(iCardToPlay), $"{nameof(iCardToPlay)} ({iCardToPlay}) is out of bounds for given cards (count: {cards.Count})");

        return validateCard(cards, iCardToPlay);
    }
}

////////////////////////////////////////////////////////////////////////////////

/// <remarks>
/// When used with <see cref="CardComboSelectionRule{TCard}"/>, <paramref name="iCardsToPlay"/>
/// can be assumed to be unique, valid indexes within <paramref name="cards"/>.
/// </remarks>
public delegate bool CardComboSelectionValidation<TCard>(
    Cards<TCard> cards,
    List<int> iCardsToPlay)
    where TCard : Card;

public readonly struct CardComboSelectionRule<TCard>(
    string description,
    CardComboSelectionValidation<TCard> validateCards)
    where TCard : Card
{
    public CardComboSelectionRule() : this(null!, null!) => throw new NotSupportedException();

    public string Description { get; } = description;

    /// <remarks>
    /// This will pre-validate <paramref name="iCardsToPlay"/> before executing the injected <see cref="CardComboSelectionValidation{TCard}"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throw when <paramref name="iCardsToPlay"/> is out of range of <paramref name="cards"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Throw when <paramref name="iCardsToPlay"/> contains duplicate values.
    /// </exception>
    public bool ValidateCards(Cards<TCard> cards, List<int> iCardsToPlay)
    {
        if (iCardsToPlay.Count != iCardsToPlay.Distinct().Count())
            throw new ArgumentException($"{nameof(iCardsToPlay)} contains at least one duplicated index");

        if (iCardsToPlay.Any(iCardToPlay => iCardToPlay < 0 || iCardToPlay >= cards.Count))
            throw new ArgumentOutOfRangeException(nameof(iCardsToPlay), $"{nameof(iCardsToPlay)} has at least one index that is out of bounds for given cards (count: {cards.Count})");

        return validateCards(cards, iCardsToPlay);
    }
}