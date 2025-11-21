namespace CoolCardGames.XUnitTests.Library.Core.CardTypes;

public class CardsTests
{
    [Fact]
    public void TestMatchesWhenEqual()
    {
        var deck = Card.MakeDeck(
        [
            Joker0.Instance,
            AceOfSpades.Instance,
            NineOfHearts.Instance,
        ]);

        var unexpectedDeck = Card.MakeDeck(
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
        var deck = Card.MakeDeck(
        [
            Joker0.Instance,
            AceOfSpades.Instance,
            NineOfHearts.Instance,
        ]);

        var unexpectedDeck = Card.MakeDeck(
        [
            NineOfHearts.Instance,
            AceOfSpades.Instance,
            Joker0.Instance,
        ]);

        Assert.False(deck.Matches(unexpectedDeck));
    }
}