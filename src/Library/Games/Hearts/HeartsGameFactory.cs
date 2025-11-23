using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;

using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoolCardGames.Library.Games.Hearts;

// TODO: since IPlayer is specific to a specific card type, need to construct one per game, so
//       could inject the channel into the player n take a player factory

public class HeartsGameFactory(
    ChannelFanOutFactory channelFanOutFactory,
    ChannelGameEventPublisherFactory channelGameEventPublisherFactory,
    IDealerFactory dealerFactory,
    IOptionsMonitor<HeartsSettings> settingsMonitor,
    ILogger<HeartsGame> logger)
{
    public HeartsSettings DefaultHeartsSettings => settingsMonitor.CurrentValue;

    public IGame Make(List<IPlayer<HeartsCard>> players, CancellationToken cancellationToken, HeartsSettings? settings = null)
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

        var eventChannel = Channel.CreateUnbounded<GameEventEnvelope>();
        var eventPublisher = channelGameEventPublisherFactory.Make(eventChannel.Writer);
        var dealer = dealerFactory.Make(eventPublisher);

        var channelFanOut = channelFanOutFactory.Make(eventChannel.Reader);
        foreach (var player in players)
        {
            var chanReader = channelFanOut.CreateReader(name: player.AccountCard.ToString());
            player.CurrentGamesEvents = chanReader;
        }

        var hearts = new HeartsGame(eventPublisher, new HeartsGameState(), players, dealer, settings, logger);
        var manager = new GameProxyChannelManager(hearts, eventChannel, channelFanOut);
        return manager;
    }
}