namespace CoolCardGames.Library.Core.CardUtils.Comparers;

// TODO: test this and the other comparer

/// <summary>
/// </summary>
/// <param name="suitPriorities">
/// Elements that are earlier in the list are considered a higher priority than elements later in the list.
/// <br />
/// If an item is not in this list, it is considered lower in priority than anything in the list.
/// </param>
public class EnumComparer<TEnum>(List<TEnum> suitPriorities) : IComparer<TEnum>
    where TEnum : struct, Enum
{
    public int Compare(TEnum x, TEnum y)
    {
        var xPriority = suitPriorities.FindIndex(suit => suit.Equals(x));
        if (xPriority == -1)
        {
            xPriority = int.MaxValue;
        }

        var yPriority = suitPriorities.FindIndex(suit => suit.Equals(y));
        if (yPriority == -1)
        {
            yPriority = int.MaxValue;
        }

        return xPriority < yPriority ? -1
            : xPriority > yPriority ? 1
            : 0;
    }
}