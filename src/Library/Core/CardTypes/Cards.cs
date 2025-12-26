using System.Runtime.InteropServices;
using System.Text;

namespace CoolCardGames.Library.Core.CardTypes;

// TODO: don't sort?
//      I like sorting so it's easier to view when debugging
//      but then diff games would need to sort differently
//      maybe have SortedCards which always auto-sorts?
//      could maybe also want to let players sort differently, but could make a PlayerView or
//      something to manage that elsewhere. Within a game, there is usually a de facto sorting style
// TODO: sorting
//      don't want to duplicate the sorting logic everywhere w/in a game
//      give Cards<T> something so it auto-sorts? have nullable so opt-in/-out?
//          this would be real awkward as-is, would need to have Cards not extend List
// TODO: implement sorting functionality (IComparer?)

public partial class Cards<TCard> : IList<TCard>
    where TCard : Card
{
    private readonly List<TCard> _cards;

    public Cards(int capacity = 0)
    {
        _cards = new List<TCard>(capacity);
    }

    public Cards(IEnumerable<TCard> seed)
    {
        _cards = [..seed];
    }

    /// <summary>
    /// This returns a span to the underlying cards.
    /// </summary>
    /// <returns></returns>
    public Span<TCard> AsSpan()
    {
        return CollectionsMarshal.AsSpan(_cards);
    }

    public void RevealAll()
    {
        foreach (var card in this)
            card.Hidden = false;
    }

    public void HideAll()
    {
        foreach (var card in this)
            card.Hidden = true;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append('[');
        for (var i = 0; i < this.Count; i++)
        {
            if (i == 0)
                builder.AppendLine();
            builder.Append(i);
            builder.Append(": ");
            builder.Append(this[i]);
            builder.AppendLine();
        }

        builder.Append(']');
        return builder.ToString();
    }

    public bool Matches(Cards<TCard> other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (Count != other.Count)
            return false;
        for (var i = 0; i < this.Count; i++)
        {
            Card thisCard = this[i];
            Card otherCard = other[i];
            if (thisCard != otherCard)
                return false;
        }

        return true;
    }
}