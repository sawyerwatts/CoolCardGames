using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Net;

using CoolCardGames.Library;
using CoolCardGames.Library.Core.MiscUtils;
using CoolCardGames.Library.Core.Players;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CoolCardGames.WebApi.Endpoints.GameSession;

// TODO: replace `ok(null)`s w/ 410s

// TODO: how clean up finished sessions (esp on exc)?

// TODO: in resp, don't have Hand w/ cards, have Card w/ Location enum

// TODO: openapi serialize enums as strings

// TODO: this assumes sticky sessions (so new events can be passed)

[ApiController]
[Route("v1/GameSession")]
public class GameSessionsController(
    ILoggerFactory loggerFactory,
    GameRegistry gameRegistry,
    AppLevelCancellationTokenHostedService appLevelCancellationTokenHostedService,
    IOptions<WebPlayer.Settings> webPlayerSettings,
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
            if (!gameRegistry.GameMetaDatas.Any(game => game.Name.Contains(gameType)))
            {
                logger.LogInformation("User entered unknown game type ({GameType})", gameType);
                var details = new ProblemDetails
                {
                    Title = $"Unknown {nameof(gameType)} given",
                    Detail = $"Unknown {nameof(gameType)} given: {gameType}",
                    Extensions = { [nameof(gameType)] = gameType },
                    Status = (int)HttpStatusCode.BadRequest,
                };
                disposable.Dispose();
                return BadRequest(details);
            }

            var playerLogger = loggerFactory.CreateLogger<WebPlayer>();
            var webPlayer = new WebPlayer(_accountCard, webPlayerSettings, playerLogger);
            var game = gameRegistry.MakeGame(gameType, webPlayer, gameCancellationToken);

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
    public async Task<ActionResult<GameSessionPlayCardResponse>> PlayCard(
        [Required] string sessionId,
        GameSessionPlayCardRequest playCardRequest,
        CancellationToken cancellationToken)
    {
        // TODO: use JWT sub claim to figure out player to target (if admin, get everything?)

        var session = Sessions.FirstOrDefault(session => session.PostResponse.SessionId == sessionId);
        if (session is null)
            return Ok(null);

        await session.WebPlayer.AnswerPromptForIndexOfCardToPlay(playCardRequest.IndexOfCardToPlay, cancellationToken);
        var resp = await session.WebPlayer.CheckIfCardSelectedWasNotValid(cancellationToken);
        return Ok(resp);
    }

    [HttpPost("{sessionId}/playCards")]
    public async Task<ActionResult<GameSessionPlayCardsResponse>> PlayCards(
        [Required] string sessionId,
        GameSessionPlayCardsRequest playCardsRequest,
        CancellationToken cancellationToken)
    {
        // TODO: use JWT sub claim to figure out player to target (if admin, get everything?)

        var session = Sessions.FirstOrDefault(session => session.PostResponse.SessionId == sessionId);
        if (session is null)
            return Ok(null);

        await session.WebPlayer.AnswerPromptForIndexesOfCardsToPlay(playCardsRequest.IndexesOfCardsToPlay, cancellationToken);
        var resp = await session.WebPlayer.CheckIfCardsSelectedWereNotValid(cancellationToken);
        return Ok(resp);
    }

    private sealed record Session(
        GameSessionPostResponse PostResponse,
        WebPlayer WebPlayer,
        Disposable CleanUp);
}