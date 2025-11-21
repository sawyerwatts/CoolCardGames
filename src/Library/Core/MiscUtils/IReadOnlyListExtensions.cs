namespace CoolCardGames.Library.Core.MiscUtils;

public static class IReadOnlyListExtensions
{
    /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})"/>
    public static int FindIndex<T>(this IReadOnlyList<T> ts, Predicate<T> match)
    {
        ArgumentNullException.ThrowIfNull(match, nameof(match));
        for (var i = 0; i < ts.Count; i++)
        {
            var t = ts[i];
            if (match(t))
                return i;
        }

        return -1;
    }
}