using CoolCardGames.Library.Core.CardTypes;
using CoolCardGames.Library.Core.CardUtils;
using CoolCardGames.Library.Core.GameEventTypes;
using CoolCardGames.Library.Core.GameSessionExceptions;
using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Options;

using Spectre.Console;

namespace CoolCardGames.Cli;

public partial class CliPlayer<TCard>(
    PlayerAccountCard playerAccountCard,
    IOptionsMonitor<CliPlayerUserSettings> userSettings,
    IOptionsMonitor<CliPlayerSystemSettings> systemSettings,
    ILogger<CliPlayer<TCard>> logger)
    : Player<TCard>(logger) where TCard : Card
{
    public override PlayerAccountCard AccountCard => playerAccountCard;

    private readonly Lock _lastEventIdLock = new();
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
    /// Since players receive game events to their <see cref="CurrentGamesEvents"/> channel once
    /// they are associated with a game and that game begins running, this method will "attach"
    /// the current thread to that channel and allow the player to view events and respond to
    /// prompts.
    /// <br />
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
        if (CurrGameEvents is null)
            throw new NoCurrentGameToAttachException($"Cannot attach the terminal's session to this CLI player because {nameof(CurrGameEvents)} is not ready to receive game events");
        using var loggingScope = logger.BeginScope("Account card {AccountCard}", AccountCard);
        logger.LogInformation("Attaching current game's events to this CLI session");

        await foreach (var envelope in CurrGameEvents.ReadAllAsync(cancellationToken))
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
    }

    private bool Handle(GameEventEnvelope envelope)
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

    protected override async Task<int> PromptForIndexOfCardToPlay(
        uint prePromptEventId,
        Cards<TCard> cards,
        List<CardSelectionRule<TCard>> cardSelectionRules,
        CancellationToken cancellationToken)
    {
        await WaitUntilUiIsSynced(prePromptEventId, cancellationToken);
        logger.LogInformation("Prompting player for a card to play");

        AnsiConsole.Write("Which card do you want to play?");
        if (cardSelectionRules.Count > 0)
            WriteRulesToConsole(cardSelectionRules.Select(rule => rule.Description));
        else
            AnsiConsole.WriteLine();

        var cardToPlay = await AnsiConsole.PromptAsync(
            new SelectionPrompt<TCard>()
                // .Title(title)
                .PageSize(1024)
#pragma warning disable CA1861
                .AddChoices(cards.ToArray()),
#pragma warning restore CA1861
            cancellationToken);
        var iCardToPlay = cards.FindIndex(card => card.Equals(cardToPlay));
        logger.LogInformation("Trying to play card {CardToPlay} at index {IndexCardToPlay}", cardToPlay, iCardToPlay);
        return iCardToPlay;
    }

    protected override async Task<List<int>> PromptForIndexesOfCardsToPlay(
        uint prePromptEventId,
        Cards<TCard> cards,
        List<CardComboSelectionRule<TCard>> cardComboSelectionRules,
        CancellationToken cancellationToken)
    {
        await WaitUntilUiIsSynced(prePromptEventId, cancellationToken);
        logger.LogInformation("Prompting player for card(s) to play");

        AnsiConsole.Write("Which card do you want to play?");
        if (cardComboSelectionRules.Count > 0)
            WriteRulesToConsole(cardComboSelectionRules.Select(rule => rule.Description));
        else
            AnsiConsole.WriteLine();

        var cardsToPlay = await AnsiConsole.PromptAsync(
            new MultiSelectionPrompt<TCard>()
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

    private static void WriteRulesToConsole(IEnumerable<string> rules)
    {
        AnsiConsole.WriteLine(" Here are the rules to follow:");
        foreach (var rule in rules)
        {
            AnsiConsole.Write("\t- ");
            AnsiConsole.WriteLine(rule);
        }
    }

    protected override Task CardSelectedWasNotValid(Cards<TCard> cards, int iCardSelected, List<string> rulesFailed, CancellationToken cancellationToken)
    {
        if (rulesFailed.Count == 0)
            throw new ArgumentException($"{nameof(rulesFailed)} should not be empty");
        AnsiConsole.WriteLine("The card selected was not valid for the following reason(s):");
        foreach (var ruleFailed in rulesFailed)
            AnsiConsole.WriteLine($"\t- {ruleFailed}");
        return Task.CompletedTask;
    }

    protected override Task CardsSelectedWereNotValid(Cards<TCard> cards, List<int> iCardsSelected, List<string> rulesFailed, CancellationToken cancellationToken)
    {
        if (rulesFailed.Count == 0)
            throw new ArgumentException($"{nameof(rulesFailed)} should not be empty");
        AnsiConsole.WriteLine("The card(s) selected were not valid for the following reason(s):");
        foreach (var ruleFailed in rulesFailed)
            AnsiConsole.WriteLine($"\t- {ruleFailed}");
        return Task.CompletedTask;
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