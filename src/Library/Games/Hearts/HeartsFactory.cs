using System.ComponentModel.DataAnnotations;

using CoolCardGames.Library.Core.CardUtils;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoolCardGames.Library.Games.Hearts;

public class HeartsFactory(
    IDealer dealer,
    IOptionsMonitor<HeartsSettings> settingsMonitor,
    ILogger<Hearts> logger)
{
    public HeartsSettings DefaultHeartsSettings => settingsMonitor.CurrentValue;

    public Hearts Make(List<PlayerSession<HeartsCard>> playerSessions, HeartsSettings? settings = null)
    {
        if (playerSessions.Count != Hearts.NumPlayers)
            throw new ArgumentException($"{nameof(playerSessions)} must have {Hearts.NumPlayers} elements, but it has {playerSessions.Count} elements");

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

        var playerIntermediates = playerSessions
            .Select((playerSession, i) => new HeartsPlayer(playerSession, gameState, i))
            .ToList()
            .AsReadOnly();

        return new Hearts(playerIntermediates, gameState, dealer, settings, logger);
    }
}