namespace CoolCardGames.Library.Core.CardTypes;

/// <remarks>
/// When sorting cards in a hand, you likely want the `Ascending` version of a rank priority.
/// Otherwise, when comparing cards to figure out which has a higher value, you likely want the
/// `Descending` version of a rank priority.
/// </remarks>
public static class CommonRankPriorities
{
    /// <inheritdoc cref="CommonRankPriorities"/>
    public static readonly List<Rank> AceHighDescending =
    [
        Rank.Ace,
        Rank.King,
        Rank.Queen,
        Rank.Jack,
        Rank.Ten,
        Rank.Nine,
        Rank.Eight,
        Rank.Seven,
        Rank.Six,
        Rank.Five,
        Rank.Four,
        Rank.Three,
        Rank.Two,
    ];

    /// <inheritdoc cref="CommonRankPriorities"/>
    public static readonly List<Rank> AceHighAscending = Enumerable.Reverse(AceHighDescending).ToList();

    /// <inheritdoc cref="CommonRankPriorities"/>
    public static readonly List<Rank> AceLowDescending =
    [
        Rank.King,
        Rank.Queen,
        Rank.Jack,
        Rank.Ten,
        Rank.Nine,
        Rank.Eight,
        Rank.Seven,
        Rank.Six,
        Rank.Five,
        Rank.Four,
        Rank.Three,
        Rank.Two,
        Rank.Ace,
    ];

    /// <inheritdoc cref="CommonRankPriorities"/>
    public static readonly List<Rank> AceLowAscending = Enumerable.Reverse(AceLowDescending).ToList();
}