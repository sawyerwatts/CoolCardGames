using CoolCardGames.Library;

using Microsoft.AspNetCore.Mvc;

namespace CoolCardGames.WebApi.Endpoints.GameTypes;

[ApiController]
[Route("GameTypes")]
public class GameTypesController(GameRegistry gameRegistry) : Controller
{
    [HttpGet]
    public ActionResult<GameTypesGetResponse> Get(CancellationToken cancellationToken)
    {
        return Ok(new GameTypesGetResponse() { GameTypes = gameRegistry.GameNames.Select(name => new GameType() { Name = name }) });
    }
}