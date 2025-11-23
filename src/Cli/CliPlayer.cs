using System.Diagnostics.Eventing.Reader;
using System.Threading.Channels;

using CoolCardGames.Library.Core.CardTypes;
using CoolCardGames.Library.Core.GameEventTypes;
using CoolCardGames.Library.Core.GameSessionExceptions;
using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Options;

using Spectre.Console;

namespace CoolCardGames.Cli;

// TODO: unit test this class

public partial class CliPlayer<TCard>(
    PlayerAccountCard playerAccountCard,
    IOptionsMonitor<CliPlayerUserSettings> userSettings,
    IOptionsMonitor<CliPlayerSystemSettings> systemSettings,
    ILogger<CliPlayer<TCard>> logger)
    : IPlayer<TCard> where TCard : Card
{
    public PlayerAccountCard AccountCard => playerAccountCard;

    public ChannelReader<GameEventEnvelope>? CurrentGamesEvents { get; set; }

    private readonly object _lastEventIdLock = new();
    private uint _lastRenderedEventId = 0;

    [LoggerMessage(Level = LogLevel.Debug, Message = "Attempting to grab lock in method {MethodName}")]
    public partial void LogGrabbingLock(string methodName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Grabbed lock in method {MethodName}")]
    public partial void LogGrabbedLock(string methodName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Released lock in method {MethodName}")]
    public partial void LogReleasedLock(string methodName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Delaying {Milliseconds} before trying to grab the lock again in method {MethodName}")]
    public partial void LogDelayingBeforeGrabbingLock(int milliseconds, string methodName);

    /// <summary>
    /// This will complete once the game completes or <see cref="CurrentGamesEvents"/> is closed.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NoCurrentGameToAttachException">
    /// This is thrown when there is no configured game to attach to.
    /// </exception>
    /// <exception cref="FailedToApplyGameEventToSessionException">
    /// Any exception received when writing the event will be thrown out of this method. Catch and
    /// reattach to the session (run this method again) as desired.
    /// </exception>
    public async Task AttachSessionToCurrentGame(CancellationToken cancellationToken)
    {
        if (CurrentGamesEvents is null)
            throw new NoCurrentGameToAttachException($"Cannot attach the terminal's session to this CLI player because {nameof(CurrentGamesEvents)} is not ready to receive game events");
        using var loggingScope = logger.BeginScope("Account card {AccountCard}", AccountCard);
        logger.LogInformation("Attaching current game's events to this CLI session");

        await foreach (var envelope in CurrentGamesEvents.ReadAllAsync(cancellationToken))
        {
            bool shouldReturn;
            LogGrabbingLock(nameof(AttachSessionToCurrentGame));
            lock (_lastEventIdLock)
            {
                LogGrabbedLock(nameof(AttachSessionToCurrentGame));
                shouldReturn = Handle(envelope);
            }

            LogReleasedLock(nameof(AttachSessionToCurrentGame));

            if (shouldReturn)
                return;

            await Task.Delay(userSettings.CurrentValue.MillisecondDelayBetweenWritingMessagesToConsole, cancellationToken);
        }

        logger.LogWarning("The current game events channel closed without the game ending normally; closing the attachment to this CLI session");

        return;

        bool Handle(GameEventEnvelope envelope)
        {
            if (envelope.GameEvent is GameEvent.GameEnded)
            {
                logger.LogInformation("The game ended; closing the attachment to this CLI session");
                _lastRenderedEventId = 0;
                return true;
            }

            try
            {
                AnsiConsole.WriteLine(envelope.GameEvent.Summary);
            }
            catch (Exception exc)
            {
                logger.LogError(exc, "An exception occurred while rendering {GameEventEnvelope} to CLI", envelope);
                try
                {
                    AnsiConsole.WriteLine("An unexpected error");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Could not write failure message to CLI");
                }

                throw new FailedToApplyGameEventToSessionException(envelope, "An exception occurred while rendering to CLI", exc);
            }
            finally
            {
                _lastRenderedEventId = envelope.Id;
            }

            return false;
        }
    }

    public async Task<int> PromptForIndexOfCardToPlay(uint prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        await WaitUntilUiIsSynced(prePromptEventId, cancellationToken);
        logger.LogInformation("Prompting player for a card to play");

        var cardToPlay = await AnsiConsole.PromptAsync(
            new SelectionPrompt<TCard>()
                .Title("Which card do you want to play?")
                .PageSize(1024)
#pragma warning disable CA1861
                .AddChoices(cards.ToArray()),
#pragma warning restore CA1861
            cancellationToken);
        var iCardToPlay = cards.FindIndex(card => card.Equals(cardToPlay));
        logger.LogInformation("Trying to play card {CardToPlay} at index {IndexCardToPlay}", cardToPlay, iCardToPlay);
        return iCardToPlay;
    }

    public async Task<List<int>> PromptForIndexesOfCardsToPlay(uint prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        await WaitUntilUiIsSynced(prePromptEventId, cancellationToken);
        logger.LogInformation("Prompting player for card(s) to play");

        var cardsToPlay = await AnsiConsole.PromptAsync(
            new MultiSelectionPrompt<TCard>()
                .Title("Which card do you want to play?")
                .PageSize(1024)
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle a card, " +
                    "[green]<enter>[/] to accept)[/]")
#pragma warning disable CA1861
                .AddChoices(cards.ToArray()),
#pragma warning restore CA1861
            cancellationToken);

        var iCardsToPlay = cards
            .Select((card, iCard) => (Card: card, Index: iCard))
            .Where(x => cardsToPlay.Contains(x.Card))
            .Select(x => x.Index)
            .ToList();

        foreach (var iCardToPlay in iCardsToPlay)
            logger.LogInformation("Trying to play card {CardToPlay} at index {IndexCardToPlay}", cards[iCardToPlay], iCardToPlay);

        return iCardsToPlay;
    }

    private async Task WaitUntilUiIsSynced(uint prePromptEventId, CancellationToken cancellationToken)
    {
        while (true)
        {
            LogGrabbingLock(nameof(WaitUntilUiIsSynced));
            lock (_lastEventIdLock)
            {
                LogGrabbedLock(nameof(WaitUntilUiIsSynced));
                logger.LogInformation(
                    "Checking if the last rendered event ID ({LastRenderedEventId}) is at least as high as the pre-prompt event ID ({PrePromptEventId})",
                    _lastRenderedEventId, prePromptEventId);
                // >= because when players make concurrent actions, the UI can pass the prePromptEventId
                if (_lastRenderedEventId >= prePromptEventId)
                    return;
            }

            LogReleasedLock(nameof(WaitUntilUiIsSynced));
            LogDelayingBeforeGrabbingLock(systemSettings.CurrentValue.MillisecondDelayBetweenCheckingIfCliIsUpToDateOnEvents, nameof(WaitUntilUiIsSynced));
            await Task.Delay(systemSettings.CurrentValue.MillisecondDelayBetweenCheckingIfCliIsUpToDateOnEvents, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}