namespace CoolCardGames.Library.Core.CardUtils;

public static class GetHighest
{
    /// <summary>
    /// This method can be used to compare enums like <see cref="Rank"/> and <see cref="Suit"/> to
    /// get the highest value within <paramref name="enumsToCheck"/> per the given
    /// <paramref name="enumPriorities"/>.
    /// </summary>
    /// <param name="enumPriorities">
    /// This starts with the highest priority element and can have additional elements in
    /// decreasing priority. This must have at least one element and may not contain
    /// duplicates.
    /// </param>
    /// <param name="enumsToCheck">
    /// At least one element in this list needs to be in <paramref name="enumPriorities"/>. This
    /// must have at least one element and may contain duplicates.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static TEnum Of<TEnum>(List<TEnum> enumPriorities, List<TEnum> enumsToCheck)
        where TEnum : struct, Enum
    {
        if (enumPriorities.Count == 0)
            throw new ArgumentException($"{nameof(enumPriorities)} cannot be an empty list");
        if (enumPriorities.Count != enumPriorities.Distinct().Count())
            throw new ArgumentException($"{nameof(enumPriorities)} cannot have duplicate values");

        if (enumsToCheck.Count == 0)
            throw new ArgumentException($"{nameof(enumsToCheck)} cannot be an empty list");

        foreach (TEnum currEnumPriority in enumPriorities)
        {
            TEnum? foundEnum = null;
            foreach (TEnum enumToCheck in enumsToCheck)
            {
                if (!enumToCheck.Equals(currEnumPriority))
                    continue;
                foundEnum = enumToCheck;
                break;
            }
            if (foundEnum.HasValue)
                return foundEnum.Value;
        }

        throw new ArgumentException($"{nameof(enumsToCheck)} contains no elements that are defined in {nameof(enumPriorities)}");
    }
}