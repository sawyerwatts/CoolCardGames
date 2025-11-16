using CoolCardGames.Library.Core.Players;

namespace CoolCardGames.Library.Games.Hearts;

public class HeartsPlayerPrompter(
    IGameEventPublisher gameEventPublisher,
    IPlayer<HeartsCard> player,
    HeartsGameState gameState,
    int gameStatePlayerIndex)
    : PlayerPrompter<HeartsCard, HeartsPlayerState, HeartsGameState>(gameEventPublisher, player, gameState, gameStatePlayerIndex);