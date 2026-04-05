using CoolCardGames.Library.Games.Hearts;

namespace CoolCardGames.Library;

public class GameRegistry
{
    // TODO: prob wanna attach factories to the result, and meta data (desc, num players, etc), but let's do that later
    public IReadOnlyList<string> GameNames { get; } =
    [
        HeartsGame.NameConst
    ];
}