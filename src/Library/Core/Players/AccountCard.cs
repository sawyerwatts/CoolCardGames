namespace CoolCardGames.Library.Core.Players;

public record AccountCard(string Id, string DisplayName)
{
    public override string ToString() => _toString;
    private readonly string _toString = $"{DisplayName} ({Id})";
}