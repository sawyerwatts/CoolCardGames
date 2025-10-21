namespace CoolCardGames.Library.Games.Hearts;

public class HeartsPlayer(
    User<HeartsCard> user,
    HeartsGameState gameState,
    int gameStatePlayerIndex)
    : Player<HeartsCard, HeartsPlayerState, HeartsGameState>(user, gameState, gameStatePlayerIndex);