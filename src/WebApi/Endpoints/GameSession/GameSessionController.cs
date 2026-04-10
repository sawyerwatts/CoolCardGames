using System.ComponentModel.DataAnnotations;
using System.Net;

using CoolCardGames.Library.Core.CardTypes;
using CoolCardGames.Library.Core.MiscUtils;
using CoolCardGames.Library.Core.Players;
using CoolCardGames.Library.Games.Hearts;

using Microsoft.AspNetCore.Mvc;

namespace CoolCardGames.WebApi.Endpoints.GameSession;

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
    private readonly CancellationToken _appCancellationToken = appLevelCancellationTokenHostedService.Token;
    private readonly List<Session> _sessions = [];

    private readonly PlayerAccountCard _accountCard = new("12345", "Froefee"); // TODO: don't hardcode

    [HttpGet]
    public ActionResult<GameSessionGetResponse> Get(CancellationToken cancellationToken)
    {
        return Ok(new GameSessionGetResponse
        {
            Items = _sessions
                .Where(session => session.OwningPlayerId == _accountCard.Id)
                .Select(session => new GameSessionGetResponseItem() { SessionId = session.Id, GameName = session.GameType, })
        });
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

        try
        {
            switch (gameType) // TODO: rm duplication b/w this and cli; put into GameRegistry?
            {
                case HeartsGame.NameConst:
                    var playerLogger = loggerFactory.CreateLogger<WebPlayer<HeartsCard>>();
                    var webPlayer = new WebPlayer<HeartsCard>(_accountCard, playerLogger);
                    var game = heartsFactory.Make(
                        players:
                        [
                            webPlayer,
                            aiPlayerFactory.Make<HeartsCard>(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 0")),
                            aiPlayerFactory.Make<HeartsCard>(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 1")),
                            aiPlayerFactory.Make<HeartsCard>(new PlayerAccountCard(Guid.NewGuid().ToString(), "AI 2")),
                        ],
                        cancellationToken: gameCancellationToken);
                    game.PlayAndDisposeInBackgroundThread(gameCancellationToken);
                    var session = new Session(
                        Id: sessionId,
                        GameType: gameType,
                        OwningPlayerId: webPlayer.AccountCard.Id,
                        WebPlayer: webPlayer, // TODO: this; change player to not be generic? how much does it really need to be generic?
                        CleanUp: disposable);
                    _sessions.Add(session);

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

        return Ok(new GameSessionPostResponse() { SessionId = sessionId });
    }

    [HttpGet("{sessionId}")]
    public ActionResult<GameSessionGetCurrentStateResponse> GetCurrentState([Required] string sessionId, CancellationToken cancellationToken)
    {
        // TODO: check+use sessionId
        // TODO: use JWT sub claim to figure out player to target (if admin, get everything?)
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

    private record Session(
        string Id,
        string GameType,
        string OwningPlayerId,
        WebPlayer<Card> WebPlayer,
        Disposable CleanUp);
}