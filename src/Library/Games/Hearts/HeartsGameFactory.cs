using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;

using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoolCardGames.Library.Games.Hearts;

public class HeartsGameFactory(
    ChannelFanOutFactory channelFanOutFactory,
    IDealerFactory dealerFactory,
    IOptionsMonitor<HeartsSettings> settingsMonitor,
    ILogger<HeartsGame> logger)
{
    public HeartsSettings DefaultHeartsSettings => settingsMonitor.CurrentValue;

    public HeartsGame Make(List<IPlayer<HeartsCard>> players, HeartsSettings? settings = null)
    {
        if (players.Count != HeartsGame.NumPlayers)
            throw new ArgumentException($"{nameof(players)} must have {HeartsGame.NumPlayers} elements, but it has {players.Count} elements");

        if (settings is null)
            settings = DefaultHeartsSettings;
        else
        {
            try
            {
                Validator.ValidateObject(settings, new ValidationContext(settings), validateAllProperties: true);
            }
            catch (ValidationException exc)
            {
                throw new ArgumentException("The given game settings failed validation", exc);
            }
        }

        var gameState = new HeartsGameState();
        gameState.Players = Enumerable.Range(0, HeartsGame.NumPlayers)
            .Select(_ => new HeartsPlayerState())
            .ToList()
            .AsReadOnly();

        var prompters = players
            .Select((player, i) => new HeartsPlayerPrompter(player, gameState, i))
            .ToList()
            .AsReadOnly();

        var gameEvents = Channel.CreateUnbounded<GameEvent>();
        var dealer = dealerFactory.Make(gameEvents.Writer);

        var channelFanOut = channelFanOutFactory.Make(gameEvents.Reader);
        foreach (var player in players)
        {
            var chanReader = channelFanOut.CreateReader(name: player.AccountCard.ToString());
            player.CurrentGamesEvents = chanReader;
        }

        return new HeartsGame(gameEvents.Writer, prompters, gameState, dealer, settings, logger);
    }
}