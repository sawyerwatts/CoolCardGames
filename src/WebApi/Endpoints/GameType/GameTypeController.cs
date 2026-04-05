using CoolCardGames.Library;

using Microsoft.AspNetCore.Mvc;

namespace CoolCardGames.WebApi.Endpoints.GameType;

[ApiController]
[Route("GameType")]
public class GameTypeController(GameRegistry gameRegistry) : Controller
{
    [HttpGet]
    public ActionResult<GameTypeGetResponse> Get(CancellationToken cancellationToken)
    {
        return Ok(new GameTypeGetResponse() { GameTypes = gameRegistry.GameNames.Select(name => new GameType() { Name = name }) });
    }
}