namespace CoolCardGames.Library.Games.HeartsGame;

public sealed record HeartsCard : Card
{
    public int Points { get; }

    public HeartsCard(CardValue value)
        : base(value)
    {
        if (value.Suit is Suit.Hearts)
            Points = 1;
        else if (value.Rank is Rank.Queen && value.Suit is Suit.Spades)
            Points = 13;
        else
            Points = 0;
    }

    public new static Cards<HeartsCard> MakeDeck(IEnumerable<CardValue> seed) =>
        new(seed.Select(cardValue => new HeartsCard(cardValue)));
}