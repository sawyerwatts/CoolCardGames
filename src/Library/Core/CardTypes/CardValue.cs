namespace CoolCardGames.Library.Core.CardTypes;

public abstract record CardValue(Rank Rank, Suit Suit)
{
    public override string ToString()
    {
        return Suit is Suit.Joker
            ? Rank.ToString()
            : $"{Rank} of {Suit}";
    }
}