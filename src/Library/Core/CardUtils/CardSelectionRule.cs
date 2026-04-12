namespace CoolCardGames.Library.Core.CardUtils;

public delegate bool CardSelectionValidation(
    Cards cards,
    int iCardToPlay);

public readonly struct CardSelectionRule(
    string description,
    CardSelectionValidation validateCard)
{
    public CardSelectionRule() : this(null!, null!) => throw new NotSupportedException();

    public string Description { get; } = description;

    /// <remarks>
    /// This will pre-validate that <paramref name="iCardToPlay"/> is in bounds before executing the
    /// injected <see cref="CardSelectionValidation"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throw when <paramref name="iCardToPlay"/> is out of range of <paramref name="cards"/>.
    /// </exception>
    public bool ValidateCard(Cards cards, int iCardToPlay)
    {
        if (iCardToPlay < 0 || iCardToPlay >= cards.Count)
            throw new ArgumentOutOfRangeException(nameof(iCardToPlay), $"{nameof(iCardToPlay)} ({iCardToPlay}) is out of bounds for given cards (count: {cards.Count})");

        return validateCard(cards, iCardToPlay);
    }
}

////////////////////////////////////////////////////////////////////////////////

/// <remarks>
/// When used with <see cref="CardComboSelectionRule"/>, <paramref name="iCardsToPlay"/>
/// can be assumed to be unique, valid indexes within <paramref name="cards"/>.
/// </remarks>
public delegate bool CardComboSelectionValidation(
    Cards cards,
    List<int> iCardsToPlay);

public readonly struct CardComboSelectionRule(
    string description,
    CardComboSelectionValidation validateCards)
{
    public CardComboSelectionRule() : this(null!, null!) => throw new NotSupportedException();

    public string Description { get; } = description;

    /// <remarks>
    /// This will pre-validate that <paramref name="iCardsToPlay"/> are unique and all in bounds
    /// before executing the injected <see cref="CardComboSelectionValidation"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throw when <paramref name="iCardsToPlay"/> is out of range of <paramref name="cards"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Throw when <paramref name="iCardsToPlay"/> contains duplicate values.
    /// </exception>
    public bool ValidateCards(Cards cards, List<int> iCardsToPlay)
    {
        if (iCardsToPlay.Count != iCardsToPlay.Distinct().Count())
            throw new ArgumentException($"{nameof(iCardsToPlay)} contains at least one duplicated index");

        if (iCardsToPlay.Any(iCardToPlay => iCardToPlay < 0 || iCardToPlay >= cards.Count))
            throw new ArgumentOutOfRangeException(nameof(iCardsToPlay), $"{nameof(iCardsToPlay)} has at least one index that is out of bounds for given cards (count: {cards.Count})");

        return validateCards(cards, iCardsToPlay);
    }
}