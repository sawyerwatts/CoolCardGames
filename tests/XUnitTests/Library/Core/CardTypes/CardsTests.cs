using CoolCardGames.Library.Core.CardUtils.Comparers;

namespace CoolCardGames.XUnitTests.Library.Core.CardTypes;

public class CardsTests
{

    [Fact]
    public void TestToStringNoElements()
    {
        var cards = new Cards();
        var actual = cards.ToString();
        Assert.Equal("[]", actual);
    }

    [Fact]
    public void TestToStringOneElement()
    {
        var cards = new Cards();
        cards.Add(new Card(AceOfClubs.Instance));
        var actual = cards.ToString();
        Assert.Equal("[\n  0: Ace of Clubs\n]", actual);
    }

    [Fact]
    public void TestToStringTwoElements()
    {
        var cards = new Cards();
        cards.Add(new Card(AceOfClubs.Instance));
        cards.Add(new Card(QueenOfSpades.Instance));
        var actual = cards.ToString();
        Assert.Equal("[\n  0: Ace of Clubs\n  1: Queen of Spades\n]", actual);
    }

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

    [Fact]
    public void TestGivenCardComparerThenExistingCardsAreSorted()
    {
        var expected = new Cards();
        expected.Add(new Card(ThreeOfClubs.Instance));
        expected.Add(new Card(FiveOfClubs.Instance));
        expected.Add(new Card(NineOfClubs.Instance));
        expected.Add(new Card(TwoOfHearts.Instance));
        expected.Add(new Card(TwoOfDiamonds.Instance));

        var cards = new Cards();
        cards.Add(new Card(NineOfClubs.Instance));
        cards.Add(new Card(TwoOfDiamonds.Instance));
        cards.Add(new Card(TwoOfHearts.Instance));
        cards.Add(new Card(FiveOfClubs.Instance));
        cards.Add(new Card(ThreeOfClubs.Instance));

        cards.CardComparer = new CardComparerSuitThenRank(
            suitPriorities:
            [
                Suit.Clubs,
                Suit.Hearts,
            ],
            rankPriorities: CommonRankPriorities.AceHighAscending);

        Assert.True(cards.Matches(expected));
    }

    [Fact]
    public void TestGivenCardComparerThenCardsAreAddedInOrder()
    {
        var expected = new Cards();
        expected.Add(new Card(ThreeOfClubs.Instance));
        expected.Add(new Card(FiveOfClubs.Instance));
        expected.Add(new Card(NineOfClubs.Instance));
        expected.Add(new Card(TwoOfHearts.Instance));
        expected.Add(new Card(TwoOfDiamonds.Instance));

        var cards = new Cards();
        cards.CardComparer = new CardComparerSuitThenRank(
            suitPriorities:
            [
                Suit.Clubs,
                Suit.Hearts,
            ],
            rankPriorities: CommonRankPriorities.AceHighAscending);

        cards.Add(new Card(NineOfClubs.Instance));
        cards.Add(new Card(TwoOfDiamonds.Instance));
        cards.Add(new Card(TwoOfHearts.Instance));
        cards.Add(new Card(FiveOfClubs.Instance));
        cards.Add(new Card(ThreeOfClubs.Instance));

        Assert.True(cards.Matches(expected));
    }

    [Fact]
    public void TestGivenCardComparerThenInsertCannotBeUsed()
    {
        var cards = new Cards();
        cards.CardComparer = new CardComparerSuitThenRank(
            suitPriorities: [],
            rankPriorities: CommonRankPriorities.AceHighAscending);
        Assert.Throws<NotSupportedException>(() => cards.Insert(0, new Card(TwoOfClubs.Instance)));
    }

    [Fact]
    public void TestGivenCardComparerThenIndexSetterCannotBeUsed()
    {
        var cards = new Cards();
        cards.CardComparer = new CardComparerSuitThenRank(
            suitPriorities: [],
            rankPriorities: CommonRankPriorities.AceHighAscending);
        Assert.Throws<NotSupportedException>(() => cards[0] = new Card(TwoOfClubs.Instance));
    }

    [Fact]
    public void TestNotGivenCardComparerThenInsertCanBeUsed()
    {
        var cards = new Cards();
        cards.Insert(0, new Card(TwoOfClubs.Instance));
        Assert.Single(cards);
        Assert.Equal(TwoOfClubs.Instance, cards[0].Value);
    }

    [Fact]
    public void TestNotGivenCardComparerThenIndexSetterCanBeUsed()
    {
        var cards = new Cards();
        cards.Add(new Card(AceOfHearts.Instance));
        cards[0] = new Card(TwoOfClubs.Instance);
        Assert.Single(cards);
        Assert.Equal(TwoOfClubs.Instance, cards[0].Value);
    }
}
