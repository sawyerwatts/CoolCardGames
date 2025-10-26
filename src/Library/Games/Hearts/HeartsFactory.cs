using System.ComponentModel.DataAnnotations;

using CoolCardGames.Library.Core.Actors;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoolCardGames.Library.Games.Hearts;

public class HeartsFactory(
    IGameEventMultiplexerFactory eventMultiplexerFactory,
    IDealerFactory dealerFactory,
    IOptionsMonitor<HeartsSettings> settingsMonitor,
    ILogger<Hearts> logger)
{
    public HeartsSettings DefaultHeartsSettings => settingsMonitor.CurrentValue;

    public Hearts Make(List<UserSession<HeartsCard>> users, HeartsSettings? settings = null)
    {
        if (users.Count != Hearts.NumPlayers)
            throw new ArgumentException($"{nameof(users)} must have {Hearts.NumPlayers} elements, but it has {users.Count} elements");

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
        gameState.Players = Enumerable.Range(0, Hearts.NumPlayers)
            .Select(_ => new HeartsPlayerState())
            .ToList()
            .AsReadOnly();

        var players = users
            .Select((user, i) => new HeartsPlayer(user, gameState, i))
            .ToList()
            .AsReadOnly();

        var eventFanOut = eventMultiplexerFactory.Make(players.Select(player => player.GameEventHandler));
        var dealer = dealerFactory.Make(eventFanOut.Handle);

        return new Hearts(eventFanOut.Handle, players, gameState, dealer, settings, logger);
    }
}