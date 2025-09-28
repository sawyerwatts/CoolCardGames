namespace CoolCardGames.XUnitTests.Library.Core.CardTypes;

public class DecksTests
{
    [Fact]
    public void TestStandard52Has52UniqueCards()
    {
        List<CardValue> deck = Decks.Standard52();
        Assert.Equal(52, deck.Count);
        Assert.Equal(52, deck.Distinct().Count());
    }

    [Fact]
    public void TestStandard54Has54UniqueCards()
    {
        List<CardValue> deck = Decks.Standard54();
        Assert.Equal(54, deck.Count);
        Assert.Equal(54, deck.Distinct().Count());
    }
}