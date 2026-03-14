using System.Threading.Channels;

namespace CoolCardGames.Library.Core.Players;

/// <remarks>
/// If you want to actually code a player, you'll want to extend <see cref="Player{TCard}"/>.
/// This interface is more for ease of proxying and testing and stuff.
/// </remarks>
public interface IPlayer<TCard>
    where TCard : Card
{
    PlayerAccountCard AccountCard { get; }

    /// <remarks>
    /// To have the player leave the game, dispose of the returned value.
    /// </remarks>
    /// <param name="currGamesEvents"></param>
    /// <param name="currGameEventPublisher"></param>
    /// <returns></returns>
    Disposable JoinGame(ChannelReader<GameEventEnvelope> currGamesEvents, IGameEventPublisher currGameEventPublisher);

    // TODO: update these docs

    /// <remarks>
    /// This will take the selected card out of the appropriate <see cref="GameState{TCard,TPlayerState}.Players"/>' <see cref="PlayerState{TCard}.Hand"/>.
    /// <br />
    /// This will publish an <see cref="GameEvent.PlayerHasTheAction"/> before prompting the player and an <see cref="GameEvent.PlayerPlayedCard{TCard}"/>
    /// once the player selects a valid card.
    /// </remarks>
    /// <param name="cards"></param>
    /// <param name="validateChosenCard">
    /// This will take the player's hand and the pre-validated in-range index of the card to
    /// play, and return true iff it is valid to play that card.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <param name="reveal"></param>
    Task<TCard> PromptForValidCardAndPlay(
        Cards<TCard> cards,
        Func<Cards<TCard>, int, bool> validateChosenCard,
        CancellationToken cancellationToken,
        bool reveal = true);

    /// <remarks>
    /// This will take the selected card(s) out of the appropriate <see cref="GameState{TCard,TPlayerState}.Players"/>' <see cref="PlayerState{TCard}.Hand"/>.
    /// <br />
    /// This will publish an <see cref="GameEvent.PlayerHasTheAction"/> before prompting the player and an <see cref="GameEvent.PlayerPlayedCards{TCard}"/>
    /// once the player selects valid card(s).
    /// </remarks>
    /// <param name="cards"></param>
    /// <param name="validateChosenCards">
    /// This will take the current player hand and the pre-validated in-range and unique indexes of
    /// the cards to play, and return true iff it is valid to play those cards.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <param name="reveal"></param>
    Task<Cards<TCard>> PromptForValidCardsAndPlay(
        Cards<TCard> cards,
        Func<Cards<TCard>, List<int>, bool> validateChosenCards,
        CancellationToken cancellationToken,
        bool reveal = true);
}

public abstract class Player<TCard> : IPlayer<TCard>
    where TCard : Card
{
    public abstract PlayerAccountCard AccountCard { get; }

    /// <remarks>
    /// This is nullable because if the player has not yet been in a game, then this will not have
    /// been initialized. Do note that once a game completes, this property may contain a non-null,
    /// but completed, channel reader.
    /// </remarks>
    protected ChannelReader<GameEventEnvelope>? CurrGameEvents { get; private set; }

    private IGameEventPublisher? _currGameEventPublisher;

    public Disposable JoinGame(ChannelReader<GameEventEnvelope> currGamesEvents, IGameEventPublisher currGameEventPublisher)
    {
        AssertNull(CurrGameEvents);
        CurrGameEvents = currGamesEvents;

        AssertNull(_currGameEventPublisher);
        _currGameEventPublisher = currGameEventPublisher;

        return new Disposable(() =>
        {
            CurrGameEvents = null;
            _currGameEventPublisher = null;
        });

        void AssertNull(object? o)
        {
            if (o is not null)
                throw new InvalidOperationException($"Player {AccountCard} cannot join a new game, they are in the middle of a game");
        }
    }

    /// <remarks>
    /// This will take the selected card out of the appropriate <see cref="GameState{TCard,TPlayerState}.Players"/>' <see cref="PlayerState{TCard}.Hand"/>.
    /// <br />
    /// This will publish an <see cref="GameEvent.PlayerHasTheAction"/> before prompting the player and an <see cref="GameEvent.PlayerPlayedCard{TCard}"/>
    /// once the player selects a valid card.
    /// </remarks>
    /// <param name="cards"></param>
    /// <param name="validateChosenCard">
    /// This will take the player's hand and the pre-validated in-range index of the card to
    /// play, and return true iff it is valid to play that card.
    /// </param>
    /// <param name="cancellationToken"></param>
    public async Task<TCard> PromptForValidCardAndPlay(
        Cards<TCard> cards,
        Func<Cards<TCard>, int, bool> validateChosenCard,
        CancellationToken cancellationToken,
        bool reveal = true)
    {
        if (_currGameEventPublisher is null)
            throw new InvalidOperationException($"Cannot prompt player {AccountCard} for a card to play, they have not joined a game");

        var syncEvent = await _currGameEventPublisher.Publish(
            gameEvent: new GameEvent.PlayerHasTheAction(AccountCard),
            cancellationToken: cancellationToken);

        var validCardToPlay = false;
        var iCardToPlay = -1;
        while (!validCardToPlay)
        {
            iCardToPlay = await PromptForIndexOfCardToPlay(syncEvent.Id, cards, cancellationToken);
            if (iCardToPlay < 0 || iCardToPlay >= cards.Count)
                continue;

            validCardToPlay = validateChosenCard(cards, iCardToPlay);
        }

        var cardToPlay = cards[iCardToPlay];
        cards.RemoveAt(iCardToPlay);

        if (reveal)
        {
            cardToPlay.Hidden = false;
        }

        if (cardToPlay.Hidden)
        {
            await _currGameEventPublisher.Publish(
                gameEvent: new GameEvent.PlayerPlayedHiddenCard(AccountCard),
                cancellationToken: cancellationToken);
        }
        else
        {
            await _currGameEventPublisher.Publish(
                gameEvent: new GameEvent.PlayerPlayedCard<TCard>(AccountCard, cardToPlay),
                cancellationToken: cancellationToken);
        }

        return cardToPlay;
    }

    /// <remarks>
    /// This will take the selected card(s) out of the appropriate <see cref="GameState{TCard,TPlayerState}.Players"/>' <see cref="PlayerState{TCard}.Hand"/>.
    /// <br />
    /// This will publish an <see cref="GameEvent.PlayerHasTheAction"/> before prompting the player and an <see cref="GameEvent.PlayerPlayedCards{TCard}"/>
    /// once the player selects valid card(s).
    /// </remarks>
    /// <param name="cards"></param>
    /// <param name="validateChosenCards">
    /// This will take the current player hand and the pre-validated in-range and unique indexes of
    /// the cards to play, and return true iff it is valid to play those cards.
    /// </param>
    /// <param name="cancellationToken"></param>
    public async Task<Cards<TCard>> PromptForValidCardsAndPlay(
        Cards<TCard> cards,
        Func<Cards<TCard>, List<int>, bool> validateChosenCards,
        CancellationToken cancellationToken,
        bool reveal = true)
    {
        if (_currGameEventPublisher is null)
            throw new InvalidOperationException($"Cannot prompt player {AccountCard} for a card to play, they have not joined a game");

        var syncEvent = await _currGameEventPublisher.Publish(
            gameEvent: new GameEvent.PlayerHasTheAction(AccountCard),
            cancellationToken: cancellationToken);

        var validCardsToPlay = false;
        List<int> iCardsToPlay = [];
        while (!validCardsToPlay)
        {
            iCardsToPlay = await PromptForIndexesOfCardsToPlay(syncEvent.Id, cards, cancellationToken);
            if (iCardsToPlay.Count != iCardsToPlay.Distinct().Count())
                continue;

            if (iCardsToPlay.Any(iCardToPlay => iCardToPlay < 0 || iCardToPlay >= cards.Count))
                continue;

            validCardsToPlay = validateChosenCards(cards, iCardsToPlay);
        }

        Cards<TCard> cardsToPlay = new(capacity: iCardsToPlay.Count);
        foreach (var iCardToPlay in iCardsToPlay.OrderDescending())
        {
            cardsToPlay.Add(cards[iCardToPlay]);
            cards.RemoveAt(iCardToPlay);
        }

        if (reveal)
        {
            foreach (var cardToPlay in cardsToPlay)
            {
                cardToPlay.Hidden = false;
            }
        }

        if (cardsToPlay.Any(card => card.Hidden))
        {
            await _currGameEventPublisher.Publish(
                gameEvent: new GameEvent.PlayerPlayedHiddenCards(AccountCard, cardsToPlay.Count(card => card.Hidden)),
                cancellationToken: cancellationToken);
        }

        if (cardsToPlay.Any(card => !card.Hidden))
        {
            await _currGameEventPublisher.Publish(
                gameEvent: new GameEvent.PlayerPlayedCards<TCard>(AccountCard, new Cards<TCard>(cardsToPlay.Where(card => !card.Hidden))),
                cancellationToken: cancellationToken);
        }

        return cardsToPlay;
    }

    /// <summary>
    /// This will ask the player for any card to play. Validation and removal from hand will be handled elsewhere.
    /// </summary>
    protected abstract Task<int> PromptForIndexOfCardToPlay(uint prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken);

    /// <summary>
    /// This will ask the player for card(s) to play. Validation and removal from hand will be handled elsewhere.
    /// </summary>
    protected abstract Task<List<int>> PromptForIndexesOfCardsToPlay(uint prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken);
}