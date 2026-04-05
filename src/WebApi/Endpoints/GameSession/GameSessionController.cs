using Microsoft.AspNetCore.Mvc;

namespace CoolCardGames.WebApi.Endpoints.GameSession;

[ApiController]
[Route("GameSession")]
public class GameSessionsController : Controller
{
    [HttpGet]
    public ActionResult<GameSessionGetResponse> Get(CancellationToken cancellationToken)
    {
        return Ok(new GameSessionGetResponse()); // TODO: this
    }

    [HttpPost]
    public ActionResult<GameSessionPostResponse> Post([FromQuery] string gameType, CancellationToken cancellationToken)
    {
        return Ok(new GameSessionPostResponse() { SessionId = Guid.NewGuid().ToString() }); // TODO: this
    }

    [HttpGet("{sessionId}/newEventsSince/{lastEventId}")]
    public ActionResult<GameSessionNewEventsResponse> GetNewEvents(string sessionId, string lastEventId, CancellationToken cancellationToken)
    {
        // TODO: this; somehow get game events into swagger too
        // TODO: the events need to indicate that the player needs to play card(s)
        // TODO: Will need to use jwt's `sub` and Redis so user can't rewind
        return Ok(new GameSessionNewEventsResponse()
        {
            LastEventsId = "3",
        });
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