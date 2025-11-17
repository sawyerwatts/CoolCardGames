using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;

using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoolCardGames.Library.Games.Hearts;

public class HeartsGameFactory(
    ChannelFanOutFactory channelFanOutFactory,
    ChannelGameEventPublisherFactory channelGameEventPublisherFactory,
    IDealerFactory dealerFactory,
    IOptionsMonitor<HeartsSettings> settingsMonitor,
    ILogger<HeartsGame> logger)
{
    public HeartsSettings DefaultHeartsSettings => settingsMonitor.CurrentValue;

    public Game<HeartsCard, HeartsPlayerState> Make(List<IPlayer<HeartsCard>> players, CancellationToken cancellationToken, HeartsSettings? settings = null)
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

        var eventEnvelopesChannel = Channel.CreateUnbounded<GameEventEnvelope>();
        var gameEventPublisher = channelGameEventPublisherFactory.Make(eventEnvelopesChannel.Writer);
        var dealer = dealerFactory.Make(gameEventPublisher);

        var channelFanOut = channelFanOutFactory.Make(eventEnvelopesChannel.Reader);
        foreach (var player in players)
        {
            var chanReader = channelFanOut.CreateReader(name: player.PlayerAccountCard.ToString());
            player.CurrentGamesEvents = chanReader;
        }

        return new HeartsGame(gameEventPublisher, new HeartsGameState(), players, dealer, settings, logger);
    }
}
