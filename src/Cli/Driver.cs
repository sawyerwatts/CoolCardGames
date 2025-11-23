using CoolCardGames.Library.Core.Players;
using CoolCardGames.Library.Games.Hearts;

using Microsoft.Extensions.Options;

using Spectre.Console;

namespace CoolCardGames.Cli;

// BUG:  Cards.Sorted should take Card ordering
//       auto sorting hands would be nice, esp on additions
//       sorting svc?
// TODO: could more hand events be auto generated instead of hardcoded in HeartsGame?
//       make a HandSvc to handle all hand ops n event pushing?
// TODO: have Hand and Trick types that publish events (and HandFactory + TrickFactory)?
//       hand played+revealed a card, trick given a card from player, etc
//           track the playing index with the card(s) played
//       hand could have sorting policy to be applied upon card addition
//       see DetermineTrickTakerIndexRelativeToStartPlayer for something to be added

// TODO: want the ability to see an overview of everything and/or refresh everything (refresh everything on attachment)
//       wanna see
//           game settings
//           cli user settings
//           hearts game state (score, num cards in other people's hands, etc)
//       does Spectre support panels?
//       prob wanna impl in CliPlayer

// TODO: revisit how game is kicked off, particularly if cliplayer craps out. don't wanna leak that thread
//       make a game cancelToken and cancel at end of loop?

// TODO: make this more dynamic instead of hardcoding hearts for everything

public class Driver(
    CliPlayerFactory cliPlayerFactory,
    AiPlayerFactory aiPlayerFactory,
    IOptionsMonitor<Driver.Settings> driverSettings,
    IServiceProvider services,
    ILogger<Driver> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        PlayerAccountCard accountCard = GetSessionsAccountCard();
        AnsiConsole.WriteLine($"Hello, {accountCard.DisplayName}!");

        string gameName = driverSettings.CurrentValue.GameName;

        while (!cancellationToken.IsCancellationRequested)
        {
            // TODO: option to customize cli user settings

            if (string.IsNullOrWhiteSpace(gameName))
            {
                gameName = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("What game do you want to play?")
#pragma warning disable CA1861
                        .AddChoices(new[] { "Hearts", })
#pragma warning restore CA1861
                );
            }

            await TryInitAndRun(gameName, accountCard, cancellationToken);
            gameName = "";
        }
    }

    private async Task TryInitAndRun(string gameName, PlayerAccountCard accountCard, CancellationToken cancellationToken)
    {
        logger.LogInformation("Initializing {Game}", gameName);
        try
        {
            var cliPlayer = cliPlayerFactory.Make<HeartsCard>(accountCard);
            var heartsFactory = services.GetRequiredService<HeartsGameFactory>(); // TODO: allow for customizing settings

            var game = heartsFactory.Make(
                players:
                [
                    cliPlayer,
                    aiPlayerFactory.Make<HeartsCard>(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 0")),
                    aiPlayerFactory.Make<HeartsCard>(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 1")),
                    aiPlayerFactory.Make<HeartsCard>(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 2")),
                ],
                cancellationToken: cancellationToken);
            _ = Task.Run(async () =>
            {
                try
                {
                    await game.Play(cancellationToken);
                }
                finally
                {
                    game.Dispose();
                }
            }, cancellationToken);

            await cliPlayer.AttachSessionToCurrentGame(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.WriteLine("");
            AnsiConsole.WriteLine("Received cancellation request, exiting game");
            logger.LogInformation("Received cancellation request");
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "An exception occurred while playing {GameName}", gameName);
            AnsiConsole.WriteLine("An unexpected error occurred, please select another game");
        }
    }

    private PlayerAccountCard GetSessionsAccountCard()
    {
        string playerName = driverSettings.CurrentValue.PlayerName;
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = AnsiConsole.Prompt(new TextPrompt<string>("What is your name?"));
        }

        var accountCard = new PlayerAccountCard(Guid.NewGuid().ToString(), playerName);
        return accountCard;
    }

    public class Settings
    {
        public string PlayerName { get; set; } = "";
        public string GameName { get; set; } = "";
    }
}