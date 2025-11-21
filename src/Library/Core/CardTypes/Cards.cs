using System.Text;

namespace CoolCardGames.Library.Core.CardTypes;

public class Cards<TCard> : List<TCard>
    where TCard : Card
{
    public Cards(int capacity = 0) : base(capacity) { }

    public Cards(IEnumerable<TCard> seed) : base(seed) { }

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