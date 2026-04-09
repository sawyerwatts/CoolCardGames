using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

namespace CoolCardGames.WebApi.Endpoints.GameSession;

// TODO: this assumes sticky sessions

[ApiController]
[Route("v1/GameSession")]
public class GameSessionsController : Controller
{
    [HttpGet]
    public ActionResult<GameSessionGetResponse> Get(CancellationToken cancellationToken)
    {
        // TODO: this
        return Ok(new GameSessionGetResponse());
    }

    [HttpPost]
    public ActionResult<GameSessionPostResponse> Post([FromQuery] string gameType, CancellationToken cancellationToken)
    {
        // TODO: this; make game and player
        return Ok(new GameSessionPostResponse() { SessionId = Guid.NewGuid().ToString() });
    }

    [HttpGet("{sessionId}")]
    public ActionResult<GameSessionGetCurrentStateResponse> GetCurrentState([Required] string sessionId, CancellationToken cancellationToken)
    {
        // TODO: check+use sessionId
        // TODO: use "JWT" sub claim to figure out player to target (if admin, get everything?)
        // TODO: actually retrieve from game; if no game, 400
        var currState = new GameSessionGetCurrentStateResponse();
        return Ok(currState);
    }

    [HttpPost("{sessionId}/playCard")]
    public ActionResult<GameSessionPlayCardResponse> PlayCard(GameSessionPlayCardRequest playCard, CancellationToken cancellationToken)
    {
        // TODO: this
        return Ok(new GameSessionPlayCardResponse());
    }

    [HttpPost("{sessionId}/playCards")]
    public ActionResult<GameSessionPlayCardsResponse> PlayCards(GameSessionPlayCardsRequest playCards, CancellationToken cancellationToken)
    {
        // TODO: this
        return Ok(new GameSessionPlayCardsResponse());
    }
}