namespace CoolCardGames.Library.Games.Hearts;

public class HeartsPlayer(
    User<HeartsCard> session,
    HeartsGameState gameState,
    int gameStatePlayerIndex)
    : Player<HeartsCard, HeartsPlayerState, HeartsGameState>(session, gameState, gameStatePlayerIndex);