using CoolCardGames.Library.Core.Actors;

namespace CoolCardGames.Library.Games.Hearts;

public class HeartsPlayer(
    UserSession<HeartsCard> userSession,
    HeartsGameState gameState,
    int gameStatePlayerIndex)
    : Player<HeartsCard, HeartsPlayerState, HeartsGameState>(userSession, gameState, gameStatePlayerIndex);