using System.Diagnostics;

using CoolCardGames.Library.Core.Players;
using CoolCardGames.Library.Games.Hearts;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library;

// TODO: use keyed services for game factories? what about setting overrides?

public class GameRegistry(
    HeartsGameFactory heartsGameFactory,
    AiPlayerFactory aiPlayerFactory,
    ILogger<GameRegistry> logger)
{
    public IReadOnlyList<MetaData> GameMetaDatas { get; } =
    [
        new(
            Name: HeartsGame.NameConst,
            MinPlayers: HeartsGame.NumPlayers,
            MaxPlayers: HeartsGame.NumPlayers),
    ];

    public IGame MakeGame(string gameName, IPlayer humanPlayer, CancellationToken cancellationToken)
    {
        return MakeGame(gameName, [humanPlayer], cancellationToken);
    }

    public IGame MakeGame(string gameName, List<IPlayer> humanPlayers, CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to build a {GameName} for players {PlayerCards}", gameName, string.Join(", ", humanPlayers.Select(player => player.AccountCard)));

        var paddedPlayers = new List<IPlayer>(humanPlayers);
        var metaData = GameMetaDatas.SingleOrDefault(metaData => metaData.Name == gameName);
        if (metaData is null)
            throw new ArgumentException($"Could not find a game with name {gameName}");

        var aiNumber = 1;
        for (int i = paddedPlayers.Count; paddedPlayers.Count < metaData.MinPlayers; i++, aiNumber++)
        {
            var aiPlayer = aiPlayerFactory.Make(new PlayerAccountCard(Guid.NewGuid().ToString(), $"AI {aiNumber}"));
            paddedPlayers.Add(aiPlayer);
        }

        return gameName switch
        {
            HeartsGame.NameConst => heartsGameFactory.Make(paddedPlayers, cancellationToken),
            _ => throw new UnreachableException($"Found meta data for game name {gameName} but couldn't build"),
        };
    }

    public record MetaData(
        string Name,
        int MinPlayers,
        int MaxPlayers,
        string Description = "");
}