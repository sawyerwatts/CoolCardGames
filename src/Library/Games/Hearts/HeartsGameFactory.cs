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

    public HeartsGame Make(List<IPlayer<HeartsCard>> users, HeartsSettings? settings = null)
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

        var inputPrompters = users
            .Select((user, i) => new HeartsInputPrompter(user, gameState, i))
            .ToList()
            .AsReadOnly();

        var gameEvents = Channel.CreateUnbounded<GameEvent>();
        var dealer = dealerFactory.Make(gameEvents.Writer);

        var channelFanOut = channelFanOutFactory.Make(gameEvents.Reader);
        foreach (var user in users)
        {
            var currUserCurrGameEventsChannel = channelFanOut.CreateReader(name: user.AccountCard.ToString());
            user.CurrentGamesEvents = currUserCurrGameEventsChannel;
        }

        return new HeartsGame(gameEvents.Writer, inputPrompters, gameState, dealer, settings, logger);
    }
}