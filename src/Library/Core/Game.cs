using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core;

/// <remarks>
/// If you want to actually code a game, you'll want to extend <see cref="Game{TCard,TPlayerState}"/>.
/// This interface is more for ease of proxying n stuff.
/// </remarks>
public interface IGame : IDisposable
{
    string Name { get; }
    Task Play(CancellationToken cancellationToken);
    void PlayAndDisposeInBackgroundThread(CancellationToken cancellationToken);
}

public abstract class Game<TCard, TPlayerState>(
    IGameEventPublisher gameEventPublisher,
    GameState<TCard, TPlayerState> gameState,
    IReadOnlyList<IPlayer<TCard>> players,
    ILogger<Game<TCard, TPlayerState>> logger)
    : IGame
    where TCard : Card
    where TPlayerState : PlayerState<TCard>
{
    public abstract string Name { get; }

    protected abstract object? SettingsToBeLogged { get; }

    public void PlayAndDisposeInBackgroundThread(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await Play(cancellationToken);
            }
            // Play() logs if an exception occurs so this doesn't need to.
            finally
            {
                try
                {
                    Dispose();
                }
                catch (Exception exc)
                {
                    logger.LogCritical(exc, "Game instance of {GameName} encountered an uncaught exception while disposing", Name);
                }
            }
        }, cancellationToken);
    }

    public async Task Play(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Beginning a game with settings {Settings}", SettingsToBeLogged);
            for (var i = 0; i < players.Count; i++)
                logger.LogInformation("Player at index {PlayerIndex} is {PlayerCard}", i, players[i].AccountCard);

            await gameEventPublisher.Publish(new GameEvent.GameStarted(Name), cancellationToken);
            await ActuallyPlay(cancellationToken);
            await gameEventPublisher.Publish(new GameEvent.GameEnded(Name, CompletedNormally: true), cancellationToken);
        }
        catch (Exception exc)
        {
            logger.LogCritical(exc, "A game crashed due to an uncaught exception");

            try
            {
                await gameEventPublisher.Publish(new GameEvent.GameEnded(Name, CompletedNormally: false), cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not publish the game completion event");
            }
        }
    }

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
    protected async Task<TCard> PromptForValidCardAndPlay(
        int iPlayer,
        Func<Cards<TCard>, int, bool> validateChosenCard,
        CancellationToken cancellationToken,
        bool reveal = true)
    {
        var player = players[iPlayer];
        var hand = gameState.Players[iPlayer].Hand;

        var syncEvent = await gameEventPublisher.Publish(
            gameEvent: new GameEvent.PlayerHasTheAction(player.AccountCard),
            cancellationToken: cancellationToken);

        var validCardToPlay = false;
        var iCardToPlay = -1;
        while (!validCardToPlay)
        {
            iCardToPlay = await player.PromptForIndexOfCardToPlay(syncEvent.Id, hand, cancellationToken);
            if (iCardToPlay < 0 || iCardToPlay >= hand.Count)
                continue;

            validCardToPlay = validateChosenCard(hand, iCardToPlay);
        }

        var cardToPlay = hand[iCardToPlay];
        hand.RemoveAt(iCardToPlay);

        if (reveal)
        {
            cardToPlay.Hidden = false;
        }

        if (cardToPlay.Hidden)
        {
            await gameEventPublisher.Publish(
                gameEvent: new GameEvent.PlayerPlayedHiddenCard(player.AccountCard),
                cancellationToken: cancellationToken);
        }
        else
        {
            await gameEventPublisher.Publish(
                gameEvent: new GameEvent.PlayerPlayedCard<TCard>(player.AccountCard, cardToPlay),
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
    /// <param name="iPlayer"></param>
    /// <param name="validateChosenCards">
    /// This will take the current player hand and the pre-validated in-range and unique indexes of
    /// the cards to play, and return true iff it is valid to play those cards.
    /// </param>
    /// <param name="cancellationToken"></param>
    protected async Task<Cards<TCard>> PromptForValidCardsAndPlay(
        int iPlayer,
        Func<Cards<TCard>, List<int>, bool> validateChosenCards,
        CancellationToken cancellationToken,
        bool reveal = true)
    {
        var player = players[iPlayer];
        var hand = gameState.Players[iPlayer].Hand;

        var syncEvent = await gameEventPublisher.Publish(
            gameEvent: new GameEvent.PlayerHasTheAction(player.AccountCard),
            cancellationToken: cancellationToken);

        var validCardsToPlay = false;
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
        foreach (var iCardToPlay in iCardsToPlay.OrderDescending())
        {
            cardsToPlay.Add(hand[iCardToPlay]);
            hand.RemoveAt(iCardToPlay);
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
            await gameEventPublisher.Publish(
                gameEvent: new GameEvent.PlayerPlayedHiddenCards(player.AccountCard, cardsToPlay.Count(card => card.Hidden)),
                cancellationToken: cancellationToken);
        }

        if (cardsToPlay.Any(card => !card.Hidden))
        {
            await gameEventPublisher.Publish(
                gameEvent: new GameEvent.PlayerPlayedCards<TCard>(player.AccountCard, new Cards<TCard>(cardsToPlay.Where(card => !card.Hidden))),
                cancellationToken: cancellationToken);
        }

        return cardsToPlay;
    }

    public abstract void Dispose();
}