namespace CoolCardGames.Library.Core.CardTypes;

public class CardComparer<TCard> : IComparer<TCard>
    where TCard : Card
{
    private readonly List<Rank> _rankPriorities;
    private readonly List<Suit>? _suitPriorities;

    /// <param name="rankPriorities">
    /// Elements that are earlier in the list are considered a higher priority than elements later in the list.
    /// </param>
    /// <param name="suitPriorities">
    /// Elements that are earlier in the list are considered a higher priority than elements later in the list.
    /// </param>
    public CardComparer(List<Rank> rankPriorities, List<Suit>? suitPriorities = null)
    {
        _rankPriorities = rankPriorities;
        _suitPriorities = suitPriorities;
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

        if (_suitPriorities is not null && x.Value.Suit != y.Value.Suit)
        {
            return CompareBySuit(x, y);
        }

        return CompareByRank(x, y);
    }

    private int CompareBySuit(Card x, Card y)
    {
        if (_suitPriorities is null)
        {
            throw new InvalidOperationException($"Cannot compare cards by suit, no suit priorities were configured (cards {x}, {y})");
        }

        var xSuitPriority = _suitPriorities.FindIndex(suit => suit == x.Value.Suit);
        if (xSuitPriority == -1)
        {
            throw new InvalidOperationException($"When checking {nameof(x)}, could not find the priority of suit {x.Value.Suit}");
        }

        var ySuitPriority = _suitPriorities.FindIndex(suit => suit == y.Value.Suit);
        if (ySuitPriority == -1)
        {
            throw new InvalidOperationException($"When checking {nameof(y)}, could not find the priority of suit {y.Value.Suit}");
        }

        return xSuitPriority < ySuitPriority ? -1
            : xSuitPriority > ySuitPriority ? 1
            : 0;
    }

    private int CompareByRank(Card x, Card y)
    {
        var xRankPriority = _rankPriorities.FindIndex(rank => rank == x.Value.Rank);
        if (xRankPriority == -1)
        {
            throw new InvalidOperationException($"When checking {nameof(x)}, could not find the priority of rank {x.Value.Rank}");
        }

        var yRankPriority = _rankPriorities.FindIndex(rank => rank == y.Value.Rank);
        if (yRankPriority == -1)
        {
            throw new InvalidOperationException($"When checking {nameof(y)}, could not find the priority of rank {y.Value.Rank}");
        }

        return xRankPriority < yRankPriority ? -1
            : xRankPriority > yRankPriority ? 1
            : 0;
    }
}