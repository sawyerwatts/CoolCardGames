using System.Runtime.InteropServices;
using System.Security.Cryptography;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.CardUtils;

public interface IDealer
{
    /// <remarks>
    /// This may or may not mutate the supplied <paramref name="deck"/>.
    /// </remarks>
    List<Cards<TCard>> ShuffleCutDeal<TCard>(Cards<TCard> deck, int numHands)
        where TCard : Card;

    /// <remarks>
    /// This may or may not mutate the supplied <paramref name="deck"/>.
    /// </remarks>
    Cards<TCard> Shuffle<TCard>(Cards<TCard> deck)
        where TCard : Card;

    /// <remarks>
    /// This may or may not mutate the supplied <paramref name="deck"/>.
    /// </remarks>
    Cards<TCard> Cut<TCard>(Cards<TCard> deck, int minNumCardsFromEdges = 1)
        where TCard : Card;

    List<Cards<TCard>> Deal<TCard>(Cards<TCard> deck, int numHands)
        where TCard : Card;
}

public interface IDealerFactory
{
    IDealer Make(GameEventHandler gameEventHandler);
}

public class DealerFactory(Dealer.IRng rng, ILogger<Dealer> logger) : IDealerFactory
{
    public IDealer Make(GameEventHandler gameEventHandler) => new Dealer(gameEventHandler, rng, logger);
}

public class Dealer(GameEventHandler gameEventHandler, Dealer.IRng rng, ILogger<Dealer> logger) : IDealer
{
    public List<Cards<TCard>> ShuffleCutDeal<TCard>(Cards<TCard> deck, int numHands)
        where TCard : Card
    {
        Cards<TCard> shuffled = Shuffle(deck);
        Cards<TCard> cut = Cut(shuffled);
        return Deal(cut, numHands);
    }

    public Cards<TCard> Shuffle<TCard>(Cards<TCard> deck)
        where TCard : Card
    {
        logger.LogInformation("Shuffling the deck");
        rng.Shuffle(CollectionsMarshal.AsSpan(deck));
        gameEventHandler.Invoke(GameEvent.DeckShuffled.Singleton);
        return deck;
    }

    public Cards<TCard> Cut<TCard>(Cards<TCard> deck, int minNumCardsFromEdges = 1)
        where TCard : Card
    {
        logger.LogInformation(
            "Cutting the deck with the cut occurring at least {MinNumCardsFromEdges} cards from the top and bottom",
            minNumCardsFromEdges);
        if (minNumCardsFromEdges < 1)
            throw new ArgumentException(
                $"{nameof(minNumCardsFromEdges)} must be positive, but was given {minNumCardsFromEdges}");
        if (deck.Count < 2)
            return deck;

        int newTopCardIndex;
        try
        {
            newTopCardIndex = rng.GetInt32(
                fromInclusive: minNumCardsFromEdges,
                toExclusive: deck.Count - minNumCardsFromEdges);
        }
        catch (ArgumentOutOfRangeException)
        {
            logger.LogError("The deck is too small to cut while not cutting the top or bottom {MinNumCardsFromEdges}", minNumCardsFromEdges);
            throw new ArgumentException(
                $"The deck is too small to cut while not cutting the top or bottom {minNumCardsFromEdges}");
        }

        IEnumerable<TCard> cardsBelowAndAtCut = deck.Take(newTopCardIndex + 1);
        IEnumerable<TCard> cardsAboveCut = deck.Skip(newTopCardIndex + 1);
        Cards<TCard> newCards = new(capacity: deck.Count);
        newCards.AddRange(cardsAboveCut);
        newCards.AddRange(cardsBelowAndAtCut);
        gameEventHandler.Invoke(GameEvent.DeckCut.Singleton);
        return newCards;
    }

    public List<Cards<TCard>> Deal<TCard>(Cards<TCard> deck, int numHands)
        where TCard : Card
    {
        logger.LogInformation("Dealing the deck to {NumHands} hands", numHands);
        if (numHands < 1)
            throw new ArgumentException(
                $"{nameof(numHands)} must be positive but given {numHands}");

        List<Cards<TCard>> hands = new(capacity: numHands);
        for (int i = 0; i < numHands; i++)
            hands.Add([]);

        CircularCounter iCurrHand = new(numHands);
        foreach (TCard currCard in deck)
        {
            hands[iCurrHand.N].Add(currCard);
            iCurrHand.Tick();
        }

        gameEventHandler.Invoke(new GameEvent.DeckDealt(numHands));
        return hands;
    }

    public interface IRng
    {
        /// <inheritdoc cref="RandomNumberGenerator.Shuffle{T}"/>
        void Shuffle<T>(Span<T> values);

        /// <inheritdoc cref="RandomNumberGenerator.GetInt32(int,int)"/>
        int GetInt32(int fromInclusive, int toExclusive);
    }

    public class Rng : IRng
    {
        public void Shuffle<T>(Span<T> values) => RandomNumberGenerator.Shuffle(values);

        public int GetInt32(int fromInclusive, int toExclusive) =>
            RandomNumberGenerator.GetInt32(fromInclusive: fromInclusive, toExclusive: toExclusive);
    }
}