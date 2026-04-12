using System.Runtime.InteropServices;
using System.Text;

namespace CoolCardGames.Library.Core.CardTypes;

public partial class Cards : IList<Card>
{
    /// <summary>
    /// When this value is not null, inserted cards will be sorted by comparer.
    /// <br />
    /// When this value is set to a non-null value, the existing cards will be sorted.
    /// </summary>
    public IComparer<Card>? CardComparer
    {
        get => _cardComparer;
        set
        {
            _cardComparer = value;
            if (value is null)
            {
                return;
            }

            _cards.Sort(_cardComparer);
        }
    }

    private IComparer<Card>? _cardComparer = null;

    private readonly List<Card> _cards;

    public Cards(int capacity = 0)
    {
        _cards = new List<Card>(capacity);
    }

    public Cards(IEnumerable<Card> seed)
    {
        _cards = [..seed];
    }

    /// <summary>
    /// This returns a span to the underlying cards.
    /// </summary>
    /// <returns></returns>
    public Span<Card> AsSpan()
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
            builder.Append("  ");
            builder.Append(i);
            builder.Append(": ");
            builder.Append(this[i]);
            builder.AppendLine();
        }

        builder.Append(']');
        return builder.ToString();
    }

    public bool Matches(Cards other)
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