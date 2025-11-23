using CoolCardGames.Library.Core.Players;
using CoolCardGames.Library.Games.Hearts;

using Microsoft.Extensions.Options;

using Spectre.Console;

namespace CoolCardGames.Cli;

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
            var heartsFactory = services.GetRequiredService<HeartsGameFactory>();

            // Ensure the game gets canceled when the player's session ends.
            using var gameCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var gameCancellationToken = gameCancellationTokenSource.Token;

            var game = heartsFactory.Make(
                players:
                [
                    cliPlayer,
                    aiPlayerFactory.Make<HeartsCard>(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 0")),
                    aiPlayerFactory.Make<HeartsCard>(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 1")),
                    aiPlayerFactory.Make<HeartsCard>(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 2")),
                ],
                cancellationToken: gameCancellationToken);
            _ = Task.Run(async () =>
            {
                try
                {
                    await game.Play(gameCancellationToken);
                }
                finally
                {
                    game.Dispose();
                }
            }, gameCancellationToken);

            await cliPlayer.AttachSessionToCurrentGame(gameCancellationToken);
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