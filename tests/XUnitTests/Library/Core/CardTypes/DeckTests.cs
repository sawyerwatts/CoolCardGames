namespace CoolCardGames.XUnitTests.Library.Core.CardTypes;

public class DecksTests
{
    [Fact]
    public void TestStandard52Has52UniqueCards()
    {
        var deck = Decks.Standard52();
        Assert.Equal(52, deck.Count);
        Assert.Equal(52, deck.Distinct().Count());
    }

    [Fact]
    public void TestStandard54Has54UniqueCards()
    {
        var deck = Decks.Standard54();
        Assert.Equal(54, deck.Count);
        Assert.Equal(54, deck.Distinct().Count());
    }
}