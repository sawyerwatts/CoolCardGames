using Microsoft.AspNetCore.Mvc;

namespace CoolCardGames.WebApi.Endpoints.GameSessions;

[ApiController]
[Route("GameSessions")]
public class GameSessionsController : Controller
{
    [HttpGet]
    public ActionResult<GameSessionsGetResponse> Get(CancellationToken cancellationToken)
    {
        return Ok(new GameSessionsGetResponse()); // TODO: this
    }

    [HttpPost]
    public ActionResult<GameSessionsPostResponse> Post([FromQuery] string gameType, CancellationToken cancellationToken)
    {
        return Ok(new GameSessionsPostResponse() { SessionId = Guid.NewGuid().ToString() }); // TODO: this
    }

    [HttpGet("{sessionId}/newEventsSince/{lastEventId}")]
    public ActionResult<GameSessionsNewEventsResponse> GetNewEvents(string sessionId, string lastEventId, CancellationToken cancellationToken)
    {
        // TODO: this; somehow get game events into swagger too
        // TODO: Will need to use jwt's `sub` and Redis so user can't rewind
        return Ok(new GameSessionsNewEventsResponse()
        {
            LastEventsId = "3",
        });
    }
}