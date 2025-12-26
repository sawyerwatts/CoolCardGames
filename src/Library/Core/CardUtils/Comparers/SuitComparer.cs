namespace CoolCardGames.Library.Core.CardUtils.Comparers;

// TODO: merge SuitComparer and RankComparer w/ GetHighest.Of in mind

// TODO: rename CardValue and Card.Value to CardFace

/// <summary>
/// </summary>
/// <param name="suitPriorities">
/// Elements that are earlier in the list are considered a higher priority than elements later in the list.
/// <br />
/// If a <see cref="Suit"/> is not in this list, it is considered lower in prioritiy than anything in the list.
/// </param>
public class SuitComparer(List<Suit> suitPriorities) : IComparer<Suit>
{
    public int Compare(Suit x, Suit y)
    {
        var xSuitPriority = suitPriorities.FindIndex(suit => suit == x);
        if (xSuitPriority == -1)
        {
            xSuitPriority = int.MaxValue;
        }

        var ySuitPriority = suitPriorities.FindIndex(suit => suit == y);
        if (ySuitPriority == -1)
        {
            ySuitPriority = int.MaxValue;
        }

        return xSuitPriority < ySuitPriority ? -1
            : xSuitPriority > ySuitPriority ? 1
            : 0;
    }
}