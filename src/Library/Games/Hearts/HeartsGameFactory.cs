using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;

using CoolCardGames.Library.Core.Players;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoolCardGames.Library.Games.Hearts;

public class HeartsGameFactory(
    ChannelFanOutHandlerFactory channelFanOutHandlerFactory,
    IDealerFactory dealerFactory,
    IOptionsMonitor<HeartsSettings> settingsMonitor,
    ILogger<HeartsGame> logger)
{
    public HeartsSettings DefaultHeartsSettings => settingsMonitor.CurrentValue;

    public HeartsGame Make(List<IPlayerSession<HeartsCard>> users, HeartsSettings? settings = null)
    {
        if (users.Count != HeartsGame.NumPlayers)
            throw new ArgumentException($"{nameof(users)} must have {HeartsGame.NumPlayers} elements, but it has {users.Count} elements");

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

        var playerPrompters = users
            .Select((user, i) => new HeartsPlayerPrompter(user, gameState, i))
            .ToList()
            .AsReadOnly();

        var gameEvents = Channel.CreateUnbounded<GameEvent>();
        var dealer = dealerFactory.Make(gameEvents.Writer);

        var channelFanOutHandler = channelFanOutHandlerFactory.Make(gameEvents.Reader);
        foreach (var user in users)
        {
            var currUserCurrGameEventsChannel = channelFanOutHandler.CreateReader(name: user.AccountCard.ToString());
            user.CurrentGamesEvents = currUserCurrGameEventsChannel;
        }

        // TODO: somehow, at some point, need to unset channels from
        //       or maybe each player has a chanReader that is registered as a mailbox for channelFanOutHandler

        return new HeartsGame(gameEvents.Writer, playerPrompters, gameState, dealer, settings, logger);
    }
}