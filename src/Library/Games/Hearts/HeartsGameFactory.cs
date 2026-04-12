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
    ILogger<HeartsGame> gameLogger,
    ILogger<GameHarness> harnessLogger)
{
    public HeartsSettings DefaultHeartsSettings => settingsMonitor.CurrentValue;

    public IGame Make(List<IPlayer> players, CancellationToken cancellationToken, HeartsSettings? settings = null)
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

        List<IDisposable> disposals = [];

        var eventChannel = Channel.CreateUnbounded<GameEventEnvelope>();
        var eventPublisher = channelGameEventPublisherFactory.Make(eventChannel.Writer);
        var dealer = dealerFactory.Make(eventPublisher);

        var channelFanOut = channelFanOutFactory.Make(eventChannel.Reader);
        foreach (var player in players)
        {
            var chanReader = channelFanOut.CreateReader(name: player.AccountCard.ToString());
            var disposal = player.JoinGame(chanReader, eventPublisher);
            disposals.Add(disposal);
        }

        var setupRound = new HeartsSetupRound(dealer, eventPublisher, players);
        var hearts = new HeartsGame(eventPublisher, new HeartsGameState(), setupRound, players, settings, gameLogger);
        var harness = new GameHarness(hearts, eventChannel, channelFanOut, harnessLogger, disposals);
        return harness;
    }
}