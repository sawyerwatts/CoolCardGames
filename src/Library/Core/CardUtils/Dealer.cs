using System.Runtime.InteropServices;
using System.Security.Cryptography;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.CardUtils;

public interface IDealer
{
    /// <remarks>
    /// This may or may not mutate the supplied <paramref name="deck"/>.
    /// </remarks>
    Task<List<Cards<TCard>>> ShuffleCutDeal<TCard>(Cards<TCard> deck, int numHands, CancellationToken cancellationToken)
        where TCard : Card;

    /// <remarks>
    /// This may or may not mutate the supplied <paramref name="deck"/>.
    /// </remarks>
    Task<Cards<TCard>> Shuffle<TCard>(Cards<TCard> deck, CancellationToken cancellationToken)
        where TCard : Card;

    /// <remarks>
    /// This may or may not mutate the supplied <paramref name="deck"/>.
    /// </remarks>
    Task<Cards<TCard>> Cut<TCard>(Cards<TCard> deck, CancellationToken cancellationToken, int minNumCardsFromEdges = 1)
        where TCard : Card;

    Task<List<Cards<TCard>>> Deal<TCard>(Cards<TCard> deck, int numHands, CancellationToken cancellationToken)
        where TCard : Card;
}

public interface IDealerFactory
{
    IDealer Make(IGameEventPublisher gameEventPublisher);
}

public class DealerFactory(Dealer.IRng rng, ILogger<Dealer> logger) : IDealerFactory
{
    public IDealer Make(IGameEventPublisher gameEventPublisher) => new Dealer(gameEventPublisher, rng, logger);
}

public class Dealer(IGameEventPublisher gameEventPublisher, Dealer.IRng rng, ILogger<Dealer> logger) : IDealer
{
    public async Task<List<Cards<TCard>>> ShuffleCutDeal<TCard>(Cards<TCard> deck, int numHands, CancellationToken cancellationToken)
        where TCard : Card
    {
        var shuffled = await Shuffle(deck, cancellationToken);
        var cut = await Cut(shuffled, cancellationToken);
        return await Deal(cut, numHands, cancellationToken);
    }

    public async Task<Cards<TCard>> Shuffle<TCard>(Cards<TCard> deck, CancellationToken cancellationToken)
        where TCard : Card
    {
        logger.LogInformation("Shuffling the deck");
        rng.Shuffle(deck.AsSpan());
        await gameEventPublisher.Publish(GameEvent.DeckShuffled.Singleton, cancellationToken);
        return deck;
    }

    public async Task<Cards<TCard>> Cut<TCard>(Cards<TCard> deck, CancellationToken cancellationToken, int minNumCardsFromEdges = 1)
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

        var cardsBelowAndAtCut = deck.Take(newTopCardIndex + 1);
        var cardsAboveCut = deck.Skip(newTopCardIndex + 1);
        Cards<TCard> newCards = new(capacity: deck.Count);
        newCards.AddRange(cardsAboveCut);
        newCards.AddRange(cardsBelowAndAtCut);
        await gameEventPublisher.Publish(GameEvent.DeckCut.Singleton, cancellationToken);
        return newCards;
    }

    public async Task<List<Cards<TCard>>> Deal<TCard>(Cards<TCard> deck, int numHands, CancellationToken cancellationToken)
        where TCard : Card
    {
        logger.LogInformation("Dealing the deck to {NumHands} hands", numHands);
        if (numHands < 1)
            throw new ArgumentException(
                $"{nameof(numHands)} must be positive but given {numHands}");

        List<Cards<TCard>> hands = new(capacity: numHands);
        for (var i = 0; i < numHands; i++)
            hands.Add([]);

        CircularCounter iCurrHand = new(numHands);
        foreach (var currCard in deck)
        {
            hands[iCurrHand.N].Add(currCard);
            iCurrHand.Tick();
        }

        await gameEventPublisher.Publish(new GameEvent.DeckDealt(numHands), cancellationToken);
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