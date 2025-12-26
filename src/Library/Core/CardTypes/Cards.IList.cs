using System.Collections;

namespace CoolCardGames.Library.Core.CardTypes;

// This file contains the proxied methods for IList, as well as any missing methods on List but not
// IList (like AddRange and FindIndex).

public partial class Cards<TCard>
    where TCard : Card
{
    public IEnumerator<TCard> GetEnumerator()
    {
        return _cards.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_cards).GetEnumerator();
    }

    public void Add(TCard item)
    {
        _cards.Add(item);
    }

    public void AddRange(IEnumerable<TCard> items)
    {
        _cards.AddRange(items);
    }

    public void Clear()
    {
        _cards.Clear();
    }

    public bool Contains(TCard item)
    {
        return _cards.Contains(item);
    }

    public void CopyTo(TCard[] array, int arrayIndex)
    {
        _cards.CopyTo(array, arrayIndex);
    }

    public bool Remove(TCard item)
    {
        return _cards.Remove(item);
    }

    public int Count => _cards.Count;

    public bool IsReadOnly => false;

    public int IndexOf(TCard item)
    {
        return _cards.IndexOf(item);
    }

    public void Insert(int index, TCard item)
    {
        _cards.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _cards.RemoveAt(index);
    }

    public TCard this[int index]
    {
        get => _cards[index];
        set => _cards[index] = value;
    }

    public int FindIndex(Predicate<TCard> match) => _cards.FindIndex(match);
}
