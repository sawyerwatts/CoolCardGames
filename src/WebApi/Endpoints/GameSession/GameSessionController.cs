using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Net;

using CoolCardGames.Library.Core.MiscUtils;
using CoolCardGames.Library.Core.Players;
using CoolCardGames.Library.Games.Hearts;

using Microsoft.AspNetCore.Mvc;

namespace CoolCardGames.WebApi.Endpoints.GameSession;

// TODO: how clean up finished sessions?

// TODO: openapi serialize enums as strings

// TODO: this assumes sticky sessions (so new events can be passed)

[ApiController]
[Route("v1/GameSession")]
public class GameSessionsController(
    ILoggerFactory loggerFactory,
    AiPlayerFactory aiPlayerFactory,
    HeartsGameFactory heartsFactory,
    AppLevelCancellationTokenHostedService appLevelCancellationTokenHostedService,
    ILogger<GameSessionsController> logger)
    : Controller
{
    private static readonly ConcurrentBag<Session> Sessions = [];

    private readonly CancellationToken _appCancellationToken = appLevelCancellationTokenHostedService.Token;

    private readonly PlayerAccountCard _accountCard = new("12345", "Froefee"); // TODO: don't hardcode

    [HttpGet]
    public ActionResult<GameSessionGetResponse> Get(CancellationToken cancellationToken)
    {
        var items = Sessions
            .Where(session => session.PostResponse.OwningPlayerId == _accountCard.Id)
            .Select(session => new GameSessionGetResponseItem() { SessionId = session.PostResponse.SessionId, GameName = session.PostResponse.GameType, });
        var resp = new GameSessionGetResponse { Items = items };
        return Ok(resp);
    }

    [HttpPost]
    public ActionResult<GameSessionPostResponse> Post([FromQuery] string gameType, CancellationToken cancellationToken)
    {
        var gameCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_appCancellationToken);
        var gameCancellationToken = gameCancellationTokenSource.Token;
        var sessionId = Guid.NewGuid().ToString();
        var loggingScope = logger.BeginScope("Preparing to create {GameType} game with sessionId {SessionId}", gameType, sessionId);
        var disposable = new Disposable(() =>
        {
            gameCancellationTokenSource.Dispose();
            loggingScope?.Dispose();
        });

        Session newSession;
        try
        {
            switch (gameType) // TODO: rm duplication b/w this and cli; put into GameRegistry?
            {
                case HeartsGame.NameConst:
                    var playerLogger = loggerFactory.CreateLogger<WebPlayer>();
                    var webPlayer = new WebPlayer(_accountCard, playerLogger);
                    var game = heartsFactory.Make(
                        players:
                        [
                            webPlayer,
                            aiPlayerFactory.Make(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 0")),
                            aiPlayerFactory.Make(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 1")),
                            aiPlayerFactory.Make(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 2")),
                        ],
                        cancellationToken: gameCancellationToken);

                    game.PlayAndDisposeInBackgroundThread(gameCancellationToken);
                    var postResp = new GameSessionPostResponse()
                    {
                        SessionId = sessionId,
                        GameType = gameType,
                        OwningPlayerId = webPlayer.AccountCard.Id,
                    };
                    newSession = new Session(
                        PostResponse: postResp,
                        WebPlayer: webPlayer,
                        CleanUp: disposable);

                    break;
                default:
                    logger.LogInformation("User entered unknown game type ({GameType})", gameType);
                    var details = new ProblemDetails
                    {
                        Title = $"Unknown {nameof(gameType)} given",
                        Detail = $"Unknown {nameof(gameType)} given: {gameType}",
                        Extensions = { [nameof(gameType)] = gameType },
                        Status = (int)HttpStatusCode.BadRequest,
                    };
                    var resp = BadRequest(details);
                    disposable.Dispose();
                    return resp;
            }
        }
        catch
        {
            disposable.Dispose();
            throw;
        }

        Sessions.Add(newSession);
        return Ok(newSession.PostResponse);
    }

    [HttpGet("{sessionId}")]
    public async Task<ActionResult<GameSessionGetCurrentStateResponse?>> GetCurrentState([Required] string sessionId, CancellationToken cancellationToken)
    {
        // TODO: use JWT sub claim to figure out player to target (if admin, get everything?)

        var session = Sessions.FirstOrDefault(session => session.PostResponse.SessionId == sessionId);
        if (session is null)
            return Ok(null);

        var resp = await session.WebPlayer.GetCurrentState(cancellationToken);
        return Ok(resp);
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

    private sealed record Session(
        GameSessionPostResponse PostResponse,
        WebPlayer WebPlayer,
        Disposable CleanUp);
}