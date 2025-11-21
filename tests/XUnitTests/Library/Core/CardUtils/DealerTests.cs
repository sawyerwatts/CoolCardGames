using CoolCardGames.Library.Core.CardUtils;
using CoolCardGames.Library.Core.GameEventTypes;

using Microsoft.Extensions.Logging.Abstractions;

namespace CoolCardGames.XUnitTests.Library.Core.CardUtils;

public sealed class DealerTests
{
    private readonly Dealer _dealer;
    private readonly RngMock _rng;
    private readonly ListGameEventPublisher _publisher;

    public DealerTests()
    {
        _publisher = new ListGameEventPublisher();
        _rng = new RngMock();
        _dealer = new Dealer(
            gameEventPublisher: _publisher,
            rng: _rng,
            logger: NullLogger<Dealer>.Instance);
    }

    [Fact]
    public async Task TestCut()
    {
        var deck = Card.MakeDeck(
        [
            Joker0.Instance,
            AceOfHearts.Instance,
            TwoOfHearts.Instance,
            ThreeOfHearts.Instance,
            FourOfHearts.Instance,
            FiveOfHearts.Instance,
            SixOfHearts.Instance,
            SevenOfHearts.Instance,
            EightOfHearts.Instance,
            NineOfHearts.Instance,
        ]);

        _rng.GetInt32Value = 3;
        deck = await _dealer.Cut(deck, CancellationToken.None);

        var expectedDeck = Card.MakeDeck(
        [
            FourOfHearts.Instance,
            FiveOfHearts.Instance,
            SixOfHearts.Instance,
            SevenOfHearts.Instance,
            EightOfHearts.Instance,
            NineOfHearts.Instance,
            Joker0.Instance,
            AceOfHearts.Instance,
            TwoOfHearts.Instance,
            ThreeOfHearts.Instance,
        ]);
        Assert.True(deck.Matches(expectedDeck));

        Assert.Single(_publisher.Events);
        Assert.Equal(GameEvent.DeckCut.Singleton, _publisher.Events[0].GameEvent);
    }

    [Fact]
    public async Task TestShuffle()
    {
        var deck = Card.MakeDeck(Decks.Standard52());
        Cards<Card> prevDeck = new(deck);

        var numChangedDecks = 0;
        for (var i = 0; i < 5; i++)
        {
            deck = await _dealer.Shuffle(deck, CancellationToken.None);
            if (!deck.Matches(prevDeck))
                numChangedDecks++;
            prevDeck = new Cards<Card>(deck);

            Assert.Equal(i+1, _publisher.Events.Count);
            Assert.Equal(GameEvent.DeckShuffled.Singleton, _publisher.Events[i].GameEvent);
        }

        Assert.True(numChangedDecks > 3);
    }

    [Fact]
    public async Task TestDeal()
    {
        var deck = Card.MakeDeck(
        [
            Joker0.Instance,
            AceOfHearts.Instance,
            TwoOfHearts.Instance,
            ThreeOfHearts.Instance,
            FourOfHearts.Instance,
            FiveOfHearts.Instance,
            SixOfHearts.Instance,
            SevenOfHearts.Instance,
            EightOfHearts.Instance,
            NineOfHearts.Instance,
        ]);

        var expectedHand0 = Card.MakeDeck(
        [
            Joker0.Instance,
            FourOfHearts.Instance,
            EightOfHearts.Instance,
        ]);

        var expectedHand1 = Card.MakeDeck(
        [
            AceOfHearts.Instance,
            FiveOfHearts.Instance,
            NineOfHearts.Instance,
        ]);

        var expectedHand2 = Card.MakeDeck(
        [
            TwoOfHearts.Instance,
            SixOfHearts.Instance,
        ]);

        var expectedHand3 = Card.MakeDeck(
        [
            ThreeOfHearts.Instance,
            SevenOfHearts.Instance,
        ]);

        var hands = await _dealer.Deal(deck, 4, CancellationToken.None);
        Assert.Equal(4, hands.Count);
        Assert.True(expectedHand0.Matches(hands[0]));
        Assert.True(expectedHand1.Matches(hands[1]));
        Assert.True(expectedHand2.Matches(hands[2]));
        Assert.True(expectedHand3.Matches(hands[3]));

        Assert.Single(_publisher.Events);
        Assert.Equal(new GameEvent.DeckDealt(4), _publisher.Events[0].GameEvent);
    }

    private class RngMock : Dealer.IRng
    {
        private readonly Dealer.Rng _rng = new();

        public void Shuffle<T>(Span<T> values) => _rng.Shuffle(values);

        public int? GetInt32Value { get; set; }

        public int GetInt32(int fromInclusive, int toExclusive) =>
            GetInt32Value ?? _rng.GetInt32(fromInclusive, toExclusive);
    }
}