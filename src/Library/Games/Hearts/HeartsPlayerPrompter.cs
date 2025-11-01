using CoolCardGames.Library.Core.Players;

namespace CoolCardGames.Library.Games.Hearts;

public class HeartsPlayerPrompter(
    PlayerSession<HeartsCard> playerSession,
    HeartsGameState gameState,
    int gameStatePlayerIndex)
    : PlayerPrompter<HeartsCard, HeartsPlayerState, HeartsGameState>(playerSession, gameState, gameStatePlayerIndex);