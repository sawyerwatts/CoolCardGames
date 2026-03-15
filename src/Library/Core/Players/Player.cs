using System.Threading.Channels;

using Microsoft.Extensions.Logging;

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

    /// <remarks>
    /// This will modify the passed <paramref name="cards"/> and return the <see cref="TCard"/> removed.
    /// <br />
    /// This will publish an <see cref="GameEvent.PlayerHasTheAction"/> before prompting the player and an <see cref="GameEvent.PlayerPlayedCard{TCard}"/>
    /// once the player selects a valid card.
    /// </remarks>
    /// <param name="cards"></param>
    /// <param name="cardSelectionRule">
    /// This will take the player's hand and the pre-validated in-range index of the card to
    /// play, and return true iff it is valid to play that card.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <param name="reveal">
    /// Reveal the returned card after removing it from <paramref name="cards"/>
    /// </param>
    Task<TCard> PromptForValidCardAndPlay(
        Cards<TCard> cards,
        CardSelectionRule<TCard> cardSelectionRule,
        CancellationToken cancellationToken,
        bool reveal = true);

    Task<TCard> PromptForValidCardAndPlay(
        Cards<TCard> cards,
        List<CardSelectionRule<TCard>> cardSelectionRules,
        CancellationToken cancellationToken,
        bool reveal = true);

    /// <remarks>
    /// This will modify the passed <paramref name="cards"/> and return the <see cref="TCard"/>(s) removed.
    /// <br />
    /// This will publish an <see cref="GameEvent.PlayerHasTheAction"/> before prompting the player and an <see cref="GameEvent.PlayerPlayedCards{TCard}"/>
    /// once the player selects valid card(s).
    /// </remarks>
    /// <param name="cards"></param>
    /// <param name="cardComboSelectionRule">
    /// This will take the current player hand and the pre-validated in-range and unique indexes of
    /// the cards to play, and return true iff it is valid to play those cards.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <param name="reveal">
    /// Reveal the returned card after removing it from <paramref name="cards"/>
    /// </param>
    Task<Cards<TCard>> PromptForValidCardsAndPlay(
        Cards<TCard> cards,
        CardComboSelectionRule<TCard> cardComboSelectionRule,
        CancellationToken cancellationToken,
        bool reveal = true);

    Task<Cards<TCard>> PromptForValidCardsAndPlay(
        Cards<TCard> cards,
        List<CardComboSelectionRule<TCard>> cardComboSelectionRules,
        CancellationToken cancellationToken,
        bool reveal = true);
}

public abstract partial class Player<TCard>(ILogger<IPlayer<TCard>> logger) : IPlayer<TCard>
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

    public Task<TCard> PromptForValidCardAndPlay(
        Cards<TCard> cards,
        CardSelectionRule<TCard> cardSelectionRule,
        CancellationToken cancellationToken,
        bool reveal = true)
    {
        return PromptForValidCardAndPlay(cards, [cardSelectionRule], cancellationToken, reveal);
    }

    public async Task<TCard> PromptForValidCardAndPlay(
        Cards<TCard> cards,
        List<CardSelectionRule<TCard>> cardSelectionRules,
        CancellationToken cancellationToken,
        bool reveal = true)
    {
        if (_currGameEventPublisher is null)
            throw new InvalidOperationException($"Cannot prompt player {AccountCard} for a card to play, they have not joined a game");

        var syncEvent = await _currGameEventPublisher.Publish(
            gameEvent: new GameEvent.PlayerHasTheAction(AccountCard),
            cancellationToken: cancellationToken);

        var iCardToPlay = -1;
        while (true)
        {
            iCardToPlay = await PromptForIndexOfCardToPlay(syncEvent.Id, cards, cardSelectionRules, cancellationToken);
            if (iCardToPlay < 0 || iCardToPlay >= cards.Count)
                continue;

            var rulesNotFollowed = new List<string>();
            foreach (var cardSelectionRule in cardSelectionRules)
            {
                var valid = cardSelectionRule.ValidateCard(cards, iCardToPlay);
                if (!valid)
                    rulesNotFollowed.Add(cardSelectionRule.Description);
            }

            if (rulesNotFollowed.Count == 0)
                break;

            LogPlayerSelectedACardThatIsNotValid(logger, AccountCard, string.Join("; ", rulesNotFollowed));
            await CardSelectedWasNotValid(cards, iCardToPlay, rulesNotFollowed, cancellationToken);
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

    public Task<Cards<TCard>> PromptForValidCardsAndPlay(
        Cards<TCard> cards,
        CardComboSelectionRule<TCard> cardComboSelectionRule,
        CancellationToken cancellationToken,
        bool reveal = true)
    {
        return PromptForValidCardsAndPlay(cards, [cardComboSelectionRule], cancellationToken, reveal);
    }

    public async Task<Cards<TCard>> PromptForValidCardsAndPlay(
        Cards<TCard> cards,
        List<CardComboSelectionRule<TCard>> cardComboSelectionRules,
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
            iCardsToPlay = await PromptForIndexesOfCardsToPlay(syncEvent.Id, cards, cardComboSelectionRules, cancellationToken);
            if (iCardsToPlay.Count != iCardsToPlay.Distinct().Count())
                continue;

            if (iCardsToPlay.Any(iCardToPlay => iCardToPlay < 0 || iCardToPlay >= cards.Count))
                continue;

            var rulesNotFollowed = new List<string>();
            foreach (var cardComboSelectionRule in cardComboSelectionRules)
            {
                var valid = cardComboSelectionRule.ValidateCards(cards, iCardsToPlay);
                if (!valid)
                    rulesNotFollowed.Add(cardComboSelectionRule.Description);
            }

            if (rulesNotFollowed.Count == 0)
                break;

            LogPlayerSelectedCardSThatAreNotValid(logger, AccountCard, string.Join("; ", rulesNotFollowed));
            await CardsSelectedWereNotValid(cards, iCardsToPlay, rulesNotFollowed, cancellationToken);
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
    protected abstract Task<int> PromptForIndexOfCardToPlay(
        uint prePromptEventId,
        Cards<TCard> cards,
        List<CardSelectionRule<TCard>> cardSelectionRules,
        CancellationToken cancellationToken);

    /// <summary>
    /// This will ask the player for card(s) to play. Validation and removal from hand will be handled elsewhere.
    /// </summary>
    protected abstract Task<List<int>> PromptForIndexesOfCardsToPlay(
        uint prePromptEventId,
        Cards<TCard> cards,
        List<CardComboSelectionRule<TCard>> cardComboSelectionRules,
        CancellationToken cancellationToken);

    /// <summary>
    /// When <see cref="PromptForValidCardAndPlay"/> notices that <see cref="PromptForIndexOfCardToPlay"/>
    /// returns an index of a card that is not valid to play, this method is called to tell the player
    /// which rules were not followed by that selection.
    /// </summary>
    protected abstract Task CardSelectedWasNotValid(Cards<TCard> cards, int iCardSelected, List<string> rulesFailed, CancellationToken cancellationToken);

    /// <summary>
    /// When <see cref="PromptForValidCardsAndPlay"/> notices that <see cref="PromptForIndexesOfCardsToPlay"/>
    /// returns index(es) of card(s) that are not valid to play, this method is called to tell the player
    /// which rules were not followed by that selection.
    /// </summary>
    protected abstract Task CardsSelectedWereNotValid(Cards<TCard> cards, List<int> iCardsSelected, List<string> rulesFailed, CancellationToken cancellationToken);

    [LoggerMessage(LogLevel.Debug, "Player {AccountCard} selected a card that is not valid: {RulesNotFollowed}")]
    static partial void LogPlayerSelectedACardThatIsNotValid(ILogger<IPlayer<TCard>> logger, PlayerAccountCard accountCard, string rulesNotFollowed);

    [LoggerMessage(LogLevel.Debug, "Player {AccountCard} selected card(s) that are not valid: {RulesNotFollowed}")]
    static partial void LogPlayerSelectedCardSThatAreNotValid(ILogger<IPlayer<TCard>> logger, PlayerAccountCard accountCard, string rulesNotFollowed);
}