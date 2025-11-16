namespace CoolCardGames.Library.Core.Players;

// TODO: write unit tests for these funcs
// TODO: update these funcs to pass additional, human-readable validation info

public class PlayerPrompter<TCard, TPlayerState, TGameState>(
    IGameEventPublisher gameEventPublisher,
    IPlayer<TCard> player,
    TGameState gameState,
    int gameStatePlayerIndex)
    where TCard : Card
    where TPlayerState : PlayerState<TCard>
    where TGameState : GameState<TCard, TPlayerState>
{
    public AccountCard AccountCard => player.AccountCard;
    public int GameStatePlayerIndex => gameStatePlayerIndex;

    private TPlayerState PlayerState => gameState.Players[gameStatePlayerIndex];
    private Cards<TCard> Hand => PlayerState.Hand;

    /// <remarks>
    /// This will take the selected card out of the appropriate <see cref="GameState{TCard,TPlayerState}.Players"/>' <see cref="PlayerState{TCard}.Hand"/>.
    /// <br />
    /// This will publish an <see cref="GameEvent.ActorHasTheAction"/> before prompting the player and an <see cref="GameEvent.ActorPlayedCard{TCard}"/>
    /// once the player selects a valid card.
    /// </remarks>
    /// <param name="validateChosenCard">
    /// This will take the current player hand and the pre-validated in-range index of the card to
    /// play, and return true iff it is valid to play that card.
    /// </param>
    /// <param name="cancellationToken"></param>
    public async Task<TCard> PlayCard(
        Func<Cards<TCard>, int, bool> validateChosenCard,
        CancellationToken cancellationToken)
    {
        var syncEvent = await gameEventPublisher.Publish(
            gameEvent: new GameEvent.ActorHasTheAction(AccountCard),
            cancellationToken: cancellationToken);

        bool validCardToPlay = false;
        int iCardToPlay = -1;
        while (!validCardToPlay)
        {
            iCardToPlay = await player.PromptForIndexOfCardToPlay(syncEvent.Id, Hand, cancellationToken);
            if (iCardToPlay < 0 || iCardToPlay >= Hand.Count)
                continue;

            validCardToPlay = validateChosenCard(Hand, iCardToPlay);
        }

        TCard cardToPlay = Hand[iCardToPlay];
        Hand.RemoveAt(iCardToPlay);

        await gameEventPublisher.Publish(
            gameEvent: new GameEvent.ActorPlayedCard<TCard>(AccountCard, cardToPlay),
            cancellationToken: cancellationToken);

        return cardToPlay;
    }

    /// <remarks>
    /// This will take the selected card(s) out of the appropriate <see cref="GameState{TCard,TPlayerState}.Players"/>' <see cref="PlayerState{TCard}.Hand"/>.
    /// <br />
    /// This will publish an <see cref="GameEvent.ActorHasTheAction"/> before prompting the player and an <see cref="GameEvent.ActorPlayedCards{TCard}"/>
    /// once the player selects valid card(s).
    /// </remarks>
    /// <param name="validateChosenCards">
    /// This will take the current player hand and the pre-validated in-range and unique indexes of
    /// the cards to play, and return true iff it is valid to play those cards.
    /// </param>
    /// <param name="cancellationToken"></param>
    public async Task<Cards<TCard>> PlayCards(
        Func<Cards<TCard>, List<int>, bool> validateChosenCards,
        CancellationToken cancellationToken)
    {
        var syncEvent = await gameEventPublisher.Publish(
            gameEvent: new GameEvent.ActorHasTheAction(AccountCard),
            cancellationToken: cancellationToken);

        bool validCardsToPlay = false;
        List<int> iCardsToPlay = [];
        while (!validCardsToPlay)
        {
            iCardsToPlay = await player.PromptForIndexesOfCardsToPlay(syncEvent.Id, Hand, cancellationToken);
            if (iCardsToPlay.Count != iCardsToPlay.Distinct().Count())
                continue;

            if (iCardsToPlay.Any(iCardToPlay => iCardToPlay < 0 || iCardToPlay >= Hand.Count))
                continue;

            validCardsToPlay = validateChosenCards(Hand, iCardsToPlay);
        }

        Cards<TCard> cardsToPlay = new(capacity: iCardsToPlay.Count);
        foreach (int iCardToPlay in iCardsToPlay.OrderDescending())
        {
            cardsToPlay.Add(Hand[iCardToPlay]);
            Hand.RemoveAt(iCardToPlay);
        }

        await gameEventPublisher.Publish(
            gameEvent: new GameEvent.ActorPlayedCards<TCard>(AccountCard, cardsToPlay),
            cancellationToken: cancellationToken);

        return cardsToPlay;
    }
}