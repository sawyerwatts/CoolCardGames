using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoolCardGames.Library.Games.Hearts;

public class HeartsFactory(
    IDealer dealer,
    IOptionsMonitor<HeartsSettings> settingsMonitor,
    ILogger<Hearts> logger)
{
    public HeartsSettings DefaultHeartsSettings => settingsMonitor.CurrentValue;

    public Hearts Make(List<User<HeartsCard>> users, HeartsSettings? settings = null)
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
            .Select((playerSession, i) => new HeartsPlayer(playerSession, gameState, i))
            .ToList()
            .AsReadOnly();

        return new Hearts(players, gameState, dealer, settings, logger);
    }
}