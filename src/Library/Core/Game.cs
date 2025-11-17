using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core;

public abstract class Game<TCard, TPlayerState>
    where TCard : Card
    where TPlayerState : PlayerState<TCard>
{
    private readonly IGameEventPublisher _gameEventPublisher;
    private readonly GameState<TCard, TPlayerState> _gameState;
    private readonly IReadOnlyList<IPlayer<TCard>> _players;
    private readonly ILogger<Game<TCard, TPlayerState>> _logger;

    protected Game(
        IGameEventPublisher gameEventPublisher,
        GameState<TCard, TPlayerState> gameState,
        IReadOnlyList<IPlayer<TCard>> players,
        ILogger<Game<TCard, TPlayerState>> logger)
    {
        _gameEventPublisher = gameEventPublisher;
        _gameState = gameState;
        _players = players;
        _logger = logger;
    }

    public abstract string Name { get; }

    public async Task<PlayResult> Play(CancellationToken cancellationToken)
    {
        try
        {
            await _gameEventPublisher.Publish(new GameEvent.GameStarted(Name), cancellationToken);
            await ActuallyPlay(cancellationToken);
            return new PlayResult();
        }
        catch (Exception exc)
        {
            _logger.LogCritical(exc, "A game crashed due to an uncaught exception");

            Exception? publishFailureException = null;
            try
            {
                await _gameEventPublisher.Publish(new GameEvent.GameEnded(Name, CompletedNormally: false), cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not publish the game completion event");
                publishFailureException = e;
            }

            return new PlayResult(exc, publishFailureException);
        }
    }

    public readonly record struct PlayResult(
        Exception? ExceptionWhenPlayingGame = null,
        Exception? ExceptionWhenFailedToPublishGameEventForFailure = null);

    /// <remarks>
    /// Implementations should send <see cref="GameEvent"/>s via <see cref="IGameEventPublisher"/>.
    /// <br />
    /// The caller has a try/catch around this method, so implementations do not need a top-level
    /// try/catch.
    /// </remarks>
    protected abstract Task ActuallyPlay(CancellationToken cancellationToken);

    /// <remarks>
    /// This will take the selected card out of the appropriate <see cref="GameState{TCard,TPlayerState}.Players"/>' <see cref="PlayerState{TCard}.Hand"/>.
    /// <br />
    /// This will publish an <see cref="GameEvent.PlayerHasTheAction"/> before prompting the player and an <see cref="GameEvent.PlayerPlayedCard{TCard}"/>
    /// once the player selects a valid card.
    /// </remarks>
    /// <param name="iPlayer"></param>
    /// <param name="validateChosenCard">
    /// This will take the player's hand and the pre-validated in-range index of the card to
    /// play, and return true iff it is valid to play that card.
    /// </param>
    /// <param name="cancellationToken"></param>
    protected async Task<TCard> PromptAndPlayCard(
        int iPlayer,
        Func<Cards<TCard>, int, bool> validateChosenCard,
        CancellationToken cancellationToken)
    {
        var player = _players[iPlayer];
        var hand = _gameState.Players[iPlayer].Hand;

        var syncEvent = await _gameEventPublisher.Publish(
            gameEvent: new GameEvent.PlayerHasTheAction(player.PlayerAccountCard),
            cancellationToken: cancellationToken);

        bool validCardToPlay = false;
        int iCardToPlay = -1;
        while (!validCardToPlay)
        {
            iCardToPlay = await player.PromptForIndexOfCardToPlay(syncEvent.Id, hand, cancellationToken);
            if (iCardToPlay < 0 || iCardToPlay >= hand.Count)
                continue;

            validCardToPlay = validateChosenCard(hand, iCardToPlay);
        }

        TCard cardToPlay = hand[iCardToPlay];
        hand.RemoveAt(iCardToPlay);

        await _gameEventPublisher.Publish(
            gameEvent: new GameEvent.PlayerPlayedCard<TCard>(player.PlayerAccountCard, cardToPlay),
            cancellationToken: cancellationToken);

        return cardToPlay;
    }

    /// <remarks>
    /// This will take the selected card(s) out of the appropriate <see cref="GameState{TCard,TPlayerState}.Players"/>' <see cref="PlayerState{TCard}.Hand"/>.
    /// <br />
    /// This will publish an <see cref="GameEvent.PlayerHasTheAction"/> before prompting the player and an <see cref="GameEvent.PlayerPlayedCards{TCard}"/>
    /// once the player selects valid card(s).
    /// </remarks>
    /// <param name="iPlayer"></param>
    /// <param name="validateChosenCards">
    /// This will take the current player hand and the pre-validated in-range and unique indexes of
    /// the cards to play, and return true iff it is valid to play those cards.
    /// </param>
    /// <param name="cancellationToken"></param>
    protected async Task<Cards<TCard>> PromptAndPlayCards(
        int iPlayer,
        Func<Cards<TCard>, List<int>, bool> validateChosenCards,
        CancellationToken cancellationToken)
    {
        var player = _players[iPlayer];
        var hand = _gameState.Players[iPlayer].Hand;

        var syncEvent = await _gameEventPublisher.Publish(
            gameEvent: new GameEvent.PlayerHasTheAction(player.PlayerAccountCard),
            cancellationToken: cancellationToken);

        bool validCardsToPlay = false;
        List<int> iCardsToPlay = [];
        while (!validCardsToPlay)
        {
            iCardsToPlay = await player.PromptForIndexesOfCardsToPlay(syncEvent.Id, hand, cancellationToken);
            if (iCardsToPlay.Count != iCardsToPlay.Distinct().Count())
                continue;

            if (iCardsToPlay.Any(iCardToPlay => iCardToPlay < 0 || iCardToPlay >= hand.Count))
                continue;

            validCardsToPlay = validateChosenCards(hand, iCardsToPlay);
        }

        Cards<TCard> cardsToPlay = new(capacity: iCardsToPlay.Count);
        foreach (int iCardToPlay in iCardsToPlay.OrderDescending())
        {
            cardsToPlay.Add(hand[iCardToPlay]);
            hand.RemoveAt(iCardToPlay);
        }

        await _gameEventPublisher.Publish(
            gameEvent: new GameEvent.PlayerPlayedCards<TCard>(player.PlayerAccountCard, cardsToPlay),
            cancellationToken: cancellationToken);

        return cardsToPlay;
    }
}