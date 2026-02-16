namespace CoolCardGames.Library.Core.CardUtils.Comparers;

/// <summary>
/// </summary>
/// <param name="priorities">
/// Elements that are earlier in the list are considered a higher priority than elements later in the list.
/// <br />
/// If an item is not in this list, it is considered lower in priority than anything in the list.
/// </param>
public class PrioritizedEnumsComparer<TEnum>(List<TEnum> priorities) : IComparer<TEnum>
    where TEnum : struct, Enum
{
    public int Compare(TEnum x, TEnum y)
    {
        var xIndex = priorities.FindIndex(suit => suit.Equals(x));
        if (xIndex == -1)
        {
            xIndex = int.MaxValue;
        }

        var yIndex = priorities.FindIndex(suit => suit.Equals(y));
        if (yIndex == -1)
        {
            yIndex = int.MaxValue;
        }

        if (xIndex < yIndex)
        {
            return -1;
        }

        return xIndex > yIndex
            ? 1
            : 0;
    }
}