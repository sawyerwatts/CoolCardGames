using CoolCardGames.Library.Core.Players;

namespace CoolCardGames.Library.Games.Hearts;

public class HeartsInputPrompter(
    IPlayer<HeartsCard> player,
    HeartsGameState gameState,
    int gameStatePlayerIndex)
    : InputPrompter<HeartsCard, HeartsPlayerState, HeartsGameState>(player, gameState, gameStatePlayerIndex);