namespace CoolCardGames.Library.Core.CardUtils.Comparers;

/// <summary>
/// </summary>
/// <param name="rankPriorities">
/// Elements that are earlier in the list are considered a higher priority than elements later in the list.
/// <br />
/// If a <see cref="Rank"/> is not in this list, it is considered lower in prioritiy than anything in the list.
/// </param>
public class RankComparer(List<Rank> rankPriorities) : IComparer<Rank>
{
    public int Compare(Rank x, Rank y)
    {
        var xRankPriority = rankPriorities.FindIndex(rank => rank == x);
        if (xRankPriority == -1)
        {
            xRankPriority = int.MaxValue;
        }

        var yRankPriority = rankPriorities.FindIndex(rank => rank == y);
        if (yRankPriority == -1)
        {
            yRankPriority = int.MaxValue;
        }

        return xRankPriority < yRankPriority ? -1
            : xRankPriority > yRankPriority ? 1
            : 0;
    }
}