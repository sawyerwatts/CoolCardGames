namespace CoolCardGames.XUnitTests.Library.Core.CardTypes;

public class CardsTests
{
    [Fact]
    public void TestMatchesWhenEqual()
    {
        Cards<Card> deck = Card.MakeDeck(
        [
            Joker0.Instance,
            AceOfSpades.Instance,
            NineOfHearts.Instance,
        ]);

        Cards<Card> unexpectedDeck = Card.MakeDeck(
        [
            Joker0.Instance,
            AceOfSpades.Instance,
            NineOfHearts.Instance,
        ]);

        Assert.True(deck.Matches(unexpectedDeck));
    }

    [Fact]
    public void TestMatchesWhenNotEqual()
    {
        Cards<Card> deck = Card.MakeDeck(
        [
            Joker0.Instance,
            AceOfSpades.Instance,
            NineOfHearts.Instance,
        ]);

        Cards<Card> unexpectedDeck = Card.MakeDeck(
        [
            NineOfHearts.Instance,
            AceOfSpades.Instance,
            Joker0.Instance,
        ]);

        Assert.False(deck.Matches(unexpectedDeck));
    }
}