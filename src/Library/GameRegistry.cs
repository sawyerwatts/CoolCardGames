using CoolCardGames.Library.Core.Players;
using CoolCardGames.Library.Games.Hearts;

namespace CoolCardGames.Library;

public class GameRegistry(
    HeartsGameFactory heartsGameFactory,
    AiPlayerFactory aiPlayerFactory)
{
    // TODO: prob wanna attach factories to the result, and meta data (desc, num players, etc), but let's do that later
    public IReadOnlyList<string> GameNames { get; } =
    [
        HeartsGame.NameConst
    ];

    public IGame MakeGame(string gameName, IPlayer humanPlayer, CancellationToken cancellationToken)
    {
        return gameName switch
        {
            HeartsGame.NameConst => heartsGameFactory.Make(
                players:
                [
                    humanPlayer,
                    aiPlayerFactory.Make(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 0")),
                    aiPlayerFactory.Make(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 1")),
                    aiPlayerFactory.Make(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 2")),
                ],
                cancellationToken: cancellationToken),
            _ => throw new ArgumentException($"Unknown game name: {gameName}", nameof(gameName)),
        };
    }
}