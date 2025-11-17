using System.Threading.Channels;

using CoolCardGames.Library.Core.CardTypes;
using CoolCardGames.Library.Core.GameEventTypes;
using CoolCardGames.Library.Core.GameSessionExceptions;
using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Options;

using Spectre.Console;

namespace CoolCardGames.Cli;

// TODO: GameEventEnvelope.Id
//       finish passing prePromptEventId through Prompt to Player and impl (ensure the UI is up to date)
//       Update diagram (prompter pushes the HasAction and PlayedCard(s) events)
//       Doc the functionality to make sure UI is up to date when getting prompted

// TODO: want the ability to see an overview of everything and/or refresh everything (refresh everything on attachment)

public class CliPlayer<TCard>(
    AccountCard accountCard,
    IOptions<CliPlayerSettings> settings,
    ILogger<CliPlayer<TCard>> logger)
    : IPlayer<TCard>
    where TCard : Card
{
    public AccountCard AccountCard => accountCard;

    public ChannelReader<GameEventEnvelope>? CurrentGamesEvents { get; set; }

    private readonly object _lastEventIdLock = new();
    private string _lastEventId = "";

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
        LogAndAnsi("Attaching current game's events to this CLI session");

        await foreach (var envelope in CurrentGamesEvents.ReadAllAsync(cancellationToken))
        {
            bool shouldReturn;
            lock (_lastEventIdLock)
            {
                shouldReturn = Handle(envelope);
            }

            if (shouldReturn)
                return;

            await Task.Delay(new TimeSpan(settings.Value.MillisecondDelayBetweenWritingMessagesToConsole), cancellationToken);
        }

        LogAndAnsi("The current game events channel closed without the game ending normally; closing the attachment to this CLI session", LogLevel.Warning);

        return;

        bool Handle(GameEventEnvelope envelope)
        {
            if (envelope.GameEvent is GameEvent.GameEnded)
            {
                LogAndAnsi("The game ended; closing the attachment to this CLI session");
                _lastEventId = "";
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
                _lastEventId = envelope.Id;
            }

            return false;
        }
    }

    private void LogAndAnsi(string message, LogLevel level = LogLevel.Information)
    {
#pragma warning disable CA2254
        logger.Log(level, message);
#pragma warning restore CA2254
    }

    public async Task<int> PromptForIndexOfCardToPlay(string prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        // TODO: only prompt once the UI's last rendered event ID equals prePromptEventId

        TCard cardToPlay = await AnsiConsole.PromptAsync(
            new SelectionPrompt<TCard>()
                .Title("Which card do you want to play?")
                .PageSize(1024)
#pragma warning disable CA1861
                .AddChoices(cards.ToArray()),
#pragma warning restore CA1861
            cancellationToken);
        int iCardToPlay = cards.FindIndex(card => card.Equals(cardToPlay));
        logger.LogInformation("Playing card {CardToPlay} at index {IndexCardToPlay}", cardToPlay, iCardToPlay);
        return iCardToPlay;
    }

    public async Task<List<int>> PromptForIndexesOfCardsToPlay(string prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken)
    {
        // TODO: only prompt once the UI's last rendered event ID equals prePromptEventId

        List<TCard> cardsToPlay = await AnsiConsole.PromptAsync(
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

        List<int> iCardsToPlay = cards
            .Select((card, iCard) => (Card: card, Index: iCard))
            .Where(x => cardsToPlay.Contains(x.Card))
            .Select(x => x.Index)
            .ToList();

        foreach (int iCardToPlay in iCardsToPlay)
            logger.LogInformation("Playing card {CardToPlay} at index {IndexCardToPlay}", cards[iCardToPlay], iCardToPlay);

        return iCardsToPlay;
    }
}