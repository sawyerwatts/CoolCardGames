namespace CoolCardGames.Library.Core.CardTypes;

public record Card(CardValue Value)
{
    public bool Hidden { get; set; } = true;

    public Card(Card card)
    {
        Value = card.Value;
        Hidden = card.Hidden;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static Cards<Card> MakeDeck(IEnumerable<CardValue> seed) =>
        new(seed.Select(cardValue => new Card(cardValue)));
}