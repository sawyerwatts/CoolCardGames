using CoolCardGames.Library.Core.Players;
using CoolCardGames.Library.Games.Hearts;

using Spectre.Console;

namespace CoolCardGames.Cli;

// BUG: the three cards that are passed are being displayed, need to keep them hidden
//      only log value if card is not Hidden, so then playing would reveal if needed?

// TODO: the warning in this file

// TODO: make this more dynamic instead of hardcoding hearts for everything

public class Driver(
    CliPlayerFactory cliPlayerFactory,
    AiPlayerFactory aiPlayerFactory,
    IServiceProvider services,
    ILogger<Driver> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        string playerName = AnsiConsole.Prompt(new TextPrompt<string>("What is your name?"));
        var accountCard = new PlayerAccountCard(Guid.NewGuid().ToString(), playerName);

        while (!cancellationToken.IsCancellationRequested)
        {
            // TODO: option to customize cli user settings

            string gameName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What game do you want to play?")
#pragma warning disable CA1861
                    .AddChoices(new[] { "Hearts", })
#pragma warning restore CA1861
            );
            logger.LogInformation("Initializing {Game}", gameName);

            var cliPlayer = cliPlayerFactory.Make<HeartsCard>(accountCard);
            var heartsFactory = services.GetRequiredService<HeartsGameFactory>(); // TODO: allow for customizing settings

            using var game = heartsFactory.Make(
                players:
                [
                    cliPlayer,
                    aiPlayerFactory.Make<HeartsCard>(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 0")),
                    aiPlayerFactory.Make<HeartsCard>(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 1")),
                    aiPlayerFactory.Make<HeartsCard>(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 2")),
                ],
                cancellationToken: cancellationToken);

            _ = Task.Run(async () => await game.Play(cancellationToken), cancellationToken);
            await cliPlayer.AttachSessionToCurrentGame(cancellationToken);
        }
    }
}