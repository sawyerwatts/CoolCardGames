using CoolCardGames.Library;
using CoolCardGames.WebApi.GameTypes;

using Microsoft.AspNetCore.Mvc;

namespace CoolCardGames.WebApi.GameTypesEndpoints;

[ApiController]
[Route("GameTypes")]
public class GameTypesController(GameRegistry gameRegistry)
{
    [HttpGet]
    public GameTypesGetResponse Get(CancellationToken cancellationToken)
    {
        return new GameTypesGetResponse() { GameTypes = gameRegistry.GameNames.Select(name => new GameType() { Name = name }) };
    }
}