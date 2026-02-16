namespace CoolCardGames.Library.Core.CardUtils.Comparers;

public class CardComparerSuitThenRank<TCard> : IComparer<TCard>
    where TCard : Card
{
    private readonly PrioritizedEnumsComparer<Rank> _rankComparer;
    private readonly PrioritizedEnumsComparer<Suit> _suitComparer;

    /// <param name="suitPriorities">
    /// Elements that are earlier in the list are considered a higher priority than elements later in the list.
    /// </param>
    /// <param name="rankPriorities">
    /// Elements that are earlier in the list are considered a higher priority than elements later in the list.
    /// </param>
    public CardComparerSuitThenRank(List<Suit> suitPriorities, List<Rank> rankPriorities)
    {
        _rankComparer = new PrioritizedEnumsComparer<Rank>(rankPriorities);
        _suitComparer = new PrioritizedEnumsComparer<Suit>(suitPriorities);
    }

    public int Compare(TCard? x, TCard? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (y is null)
        {
            return 1;
        }

        if (x is null)
        {
            return -1;
        }

        return x.Value.Suit != y.Value.Suit
            ? _suitComparer.Compare(x.Value.Suit, y.Value.Suit)
            : _rankComparer.Compare(x.Value.Rank, y.Value.Rank);
    }
}